// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using BertaBot.CustomVision;
using BertaBot.Infrastructure;
using BertaBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.BotBuilderSamples;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BertaBot.Vehicles;
using BertaBot.Vehicles.Models;

namespace BertaBot.Bots
{
    public class UserProfileDialog : ComponentDialog
    {
        private static BotSettings _settings;
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;
        private readonly IConfiguration _config;

        private readonly CogServicesHttp _cogServices;
        private readonly LocalHttpClient _localService;
        private readonly InventoryHttp _inventoryService;

        private static ILogger<UserProfileDialog> _logger;

        public UserProfileDialog(UserState userState,
            IConfiguration config,
            ILogger<UserProfileDialog> logger,
            CogServicesHttp cogServicesHttp,
            LocalHttpClient localService,
            InventoryHttp inventoryService)
            : base(nameof(UserProfileDialog))
        {
            _logger = logger;
            _cogServices = cogServicesHttp;
            _localService = localService;
            _inventoryService = inventoryService;

            _config = config;
            _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");

            PreparePipeline();
            _settings = JsonConvert.DeserializeObject<BotSettings>(_config["botSettings"]);
        }

        void PreparePipeline()
        {
            // This array defines how the Waterfall will execute.
            var waterfallSteps = new WaterfallStep[]
            {
                ActionStep,
                ActionOptionSelected,
                GetCarImageAsync,
                CheckCarPredictionAsync,
                ConfirmStepAsync,
                SummaryStepAsync,
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>), AgePromptValidatorAsync));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new AttachmentPrompt(nameof(AttachmentPrompt)));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private static async Task<DialogTurnResult> ActionStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.SendTypingAsync(1500, cancellationToken);
            var cardOptions = new List<Choice>()
            {
                new Choice() { Value = _settings.AddCar, Synonyms = _settings.AddCarSynonyms },
                new Choice() { Value = _settings.SeeInventory, Synonyms = _settings.SeeInventorySynonyms },
                new Choice() { Value = _settings.Exit, Synonyms = _settings.ExitSynonyms },
            };

            await stepContext.SendTypingAsync(_settings.WelcomeMessage, 1000, cancellationToken);
            await stepContext.SendTypingAsync(_settings.InternWarning, 1000, cancellationToken);

            _logger.LogInformation(LoggingEvents.ActionStep, "Display options");

            return await stepContext.PromptTypingAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text(_settings.WhatToDo),
                    Choices = cardOptions,
                }, 1300, cancellationToken);
        }

        private static async Task<DialogTurnResult> ActionOptionSelected(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string value = ((FoundChoice)stepContext.Result).Value;

            _logger.LogInformation(LoggingEvents.ActionOptionSelected, "selected option = {value}", value);

            switch (value)
            {
                case "Add a new car":
                    _logger.LogInformation(LoggingEvents.ActionOptionSelected, "ask for images");
                    return await stepContext.PromptAsync(nameof(AttachmentPrompt),
                        new PromptOptions
                        {
                            Prompt = MessageFactory.Text(_settings.AskForImages)
                        }
                        , cancellationToken);
                case "See inventory":
                    var attachments = new List<Attachment>();

                    attachments.Add(GetHeroCard("BMW x1 foto", "https://imgd.aeplcdn.com/1056x594/cw/ec/20227/BMW-X1-New-Right-Front-Three-Quarter-57824.jpg").ToAttachment());
                    attachments.Add(GetHeroCard("BMW x1 branca", "https://imgd.aeplcdn.com/1056x594/cw/ec/20227/BMW-X1-Front-view-65924.jpg").ToAttachment());
                    attachments.Add(GetHeroCard("BMW x1 azul", "https://imgd.aeplcdn.com/1056x594/cw/ec/20227/BMW-X1-Right-Front-Three-Quarter-65929.jpg").ToAttachment());

                    _logger.LogInformation(LoggingEvents.ActionOptionSelected, "display available cars");

                    await stepContext.SendTypingAsync(MessageFactory.Carousel(attachments), 1000, cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(new List<string>(), cancellationToken);
                case "Exit":
                    _logger.LogInformation(LoggingEvents.ActionOptionSelected, "exiting");
                    await stepContext.Context.SendActivityAsync("ok then, bye!", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(new List<string>(), cancellationToken);
                default:
                    _logger.LogInformation(LoggingEvents.ActionOptionSelected, "invalid option");
                    await stepContext.Context.SendActivityAsync("nao entendi, vai ter que comecar tudo de novo! ainda estamos melhorando isso", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(new List<string>(), cancellationToken);
            }
        }

        private async Task<DialogTurnResult> GetCarImageAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.SendTypingAsync(_settings.CarsReceived, 1000, cancellationToken);

            var images = (List<Attachment>)stepContext.Result;

            _logger.LogInformation(LoggingEvents.GetCarImageAsync, $"{images.Count} images received");

            var predictions = await MakePredictionRequest(images.ToList(), stepContext, cancellationToken);

            _logger.LogInformation($"ordering predictions");
            predictions = predictions.OrderByDescending(x => x.probability).ToList();

            SetVehicleBrandColor(predictions);

            _logger.LogInformation($"get first prediction item");
            var vehicle = predictions.FirstOrDefault(p => p.isModel);

            //var color = asd.predictions.FirstOrDefault(p => !p.isModel);

            await stepContext.SendTypingAsync(100, cancellationToken);
            stepContext.Values["brand"] = vehicle.brand;
            stepContext.Values["model"] = vehicle.model;
            _logger.LogInformation(LoggingEvents.GetCarImageAsync, $"ask user to confirm the vehicle name prediction");
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text(_settings.CheckAnswer.Replace("#VEHICLE", vehicle.tagName))
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> CheckCarPredictionAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!(bool)stepContext.Result)
            {
                // User said "yes" so we will be prompting for the age.
                // WaterfallStep always finishes with the end of the Waterfall or with another dialog, here it is a Prompt Dialog.
                var promptOptions = new PromptOptions
                {
                    Prompt = MessageFactory.Text(_settings.WrongAnswer),
                    //RetryPrompt = MessageFactory.Text("The value entered must be greater than 0 and less than 150."),
                };

                _logger.LogInformation(LoggingEvents.CheckCarPredictionAsync, $"wrong prediction, ask for input");
                return await stepContext.PromptTypingAsync(nameof(TextPrompt), promptOptions, 1000, cancellationToken);
            }
            else
            {
                _logger.LogInformation(LoggingEvents.CheckCarPredictionAsync, $"correct prediction");
                await stepContext.SendTypingAsync(MessageFactory.ContentUrl("https://media.giphy.com/media/ckeHl52mNtoq87veET/giphy.gif", "image/gif"), 500, cancellationToken);

                var carModel = new CarModel
                {
                    Brand = (string)stepContext.Values["brand"],
                    Name = (string)stepContext.Values["model"],
                };


                var imageBytes = (List<(byte[], string)>)stepContext.Values["vehicleImages"];

                await _inventoryService.AddVehicle(carModel, imageBytes, cancellationToken: cancellationToken);
                await stepContext.SendTypingAsync(_settings.CorrectAnwer, 1000, cancellationToken);



                // User said "no" so we will skip the next step. Give -1 as the age.
                return await stepContext.NextAsync(string.Empty, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string correctVehicleName = (string)stepContext.Result;
            //stepContext.Values["vehicle"] = corr

            _logger.LogInformation($"user input = {correctVehicleName}");
            if (string.IsNullOrEmpty(correctVehicleName))//the prediction was correct
            {
                _logger.LogInformation($"the prediction was correct, there is no need to call the api");
                //await stepContext.SendTypingAsync(MessageFactory.Text("Nice, saving it now."), 1000, cancellationToken);
            }
            else
            {
                _logger.LogInformation($"wrong prediction, save it to JOAO api");
                await stepContext.SendTypingAsync(MessageFactory.Text("Nice, saving it now."), 1000, cancellationToken);
                //send to joao


            }

            _logger.LogInformation($"end of game");
            await stepContext.SendTypingAsync(MessageFactory.ContentUrl("https://pbs.twimg.com/profile_images/649600410525679616/QjoMCmpB_400x400.png", "image/png"), 1200, cancellationToken);
            await stepContext.SendTypingAsync(MessageFactory.Text("Thanks for using Cox Autoinc"), 1000, cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                // Get the current profile object from user state.
                var userProfile = await _userProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile(), cancellationToken);

                userProfile.Transport = (string)stepContext.Values["transport"];
                userProfile.Name = (string)stepContext.Values["name"];
                userProfile.Age = (int)stepContext.Values["age"];

                var msg = $"I have your mode of transport as {userProfile.Transport} and your name as {userProfile.Name}.";
                if (userProfile.Age != -1)
                {
                    msg += $" And age as {userProfile.Age}.";
                }

                await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thanks. Your profile will not be kept."), cancellationToken);
            }

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog, here it is the end.
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private static Task<bool> AgePromptValidatorAsync(PromptValidatorContext<int> promptContext, CancellationToken cancellationToken)
        {
            // This condition is our validation rule. You can also change the value at this point.
            return Task.FromResult(promptContext.Recognized.Succeeded && promptContext.Recognized.Value > 0 && promptContext.Recognized.Value < 150);
        }

        public async Task<List<Prediction>> MakePredictionRequest(List<Attachment> attchaments, WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation(LoggingEvents.MakePredictionRequest, "Starting make predictions");

            var predictions = new List<Prediction>();
            bool isSkype = string.Compare("skype", stepContext.Context.Activity.ChannelId, true) == 0;

            var imagesBytes = new List<(byte[], string)>();
            for (int i = 0; i < attchaments.Count; i++)
            {
                var (predictionsLocal, image) = await MakePredictionsWithFile(attchaments[i], i, stepContext, isSkype, cancellationToken);
                predictions.AddRange(predictionsLocal.OrderByDescending(x => x.probability).Take(4));
                imagesBytes.Add((image, attchaments[i].ContentType));
            }

            await stepContext.SendTypingAsync(_settings.ProcessingFinished, 1000, cancellationToken: cancellationToken);
            await stepContext.SendTypingAsync(_settings.Complain, 1000, cancellationToken: cancellationToken);
            stepContext.Values["vehicleImages"] = imagesBytes;
            return predictions;
        }

        async Task<(List<Prediction>, byte[])> MakePredictionsWithFile(Attachment attachment, int index, WaterfallStepContext stepContext, bool isSkype = false, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Sending Prediction using localhost for image {IMAGE_URI}", attachment.ContentUrl);
            try
            {
                var image = await _localService.GetImageAsync(attachment, stepContext.Context.Activity, isSkype);

                var result = await _cogServices.MakePredictionAsync(image);

                await stepContext.SendTypingAsync(_settings.Processing.Replace("#NUMBER", (index).ToString()), 1500, cancellationToken: cancellationToken);

                return (result.predictions, image);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "error when sending through localhost {IMAGE_URI}", attachment.ContentUrl);
                throw;
            }

        }

        //async Task<List<Prediction>> MakePredictionsHttpV2(string imageUri, int index, WaterfallStepContext stepContext, CancellationToken cancellationToken = default)
        //{
        //    _logger.LogInformation("Sending Prediction using target href for image {IMAGE_URI}", imageUri);

        //    try
        //    {
        //        var result = await _cogServices.MakePredictionUriAsync(imageUri);

        //        await stepContext.SendTypingAsync(_settings.Processing.Replace("#NUMBER", (index).ToString()), 1500, cancellationToken: cancellationToken);

        //        return result.predictions;
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError(e, "error when sending through href target {IMAGE_URI}", imageUri);
        //        throw;
        //    }
        //}

        private static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }

        private static HeroCard GetHeroCard(string title, string imageUri)
        {
            var heroCard = new HeroCard
            {
                Title = title,
                Images = new List<CardImage> { new CardImage(imageUri) }
                //Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Get Started", value: "https://docs.microsoft.com/bot-framework") },
            };

            return heroCard;
        }


        private void SetVehicleBrandColor(List<Prediction> predictions)
        {
            var data = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            data.Add("500", "Fiat");
            data.Add("Palio", "Fiat");
            data.Add("Doblo", "Fiat");
            data.Add("Uno", "Fiat");

            data.Add("116i", "BMW");
            data.Add("550i", "BMW");
            data.Add("X1", "BMW");

            data.Add("Ecosport", "Ford");
            data.Add("Fusion", "Ford");
            data.Add("Ranger", "Ford");

            data.Add("Fusca", "Volkswagen");
            data.Add("Golf", "Volkswagen");

            data.Add("Hilux", "Toyota");
            data.Add("Corolla", "Toyota");

            data.Add("i30", "Hyundai");
            data.Add("HB20", "Hyundai");

            data.Add("Versa", "Nissan");
            data.Add("March", "Nissan");

            data.Add("ML 350", "Mercedes");
            data.Add("SLC 300", "Mercedes");

            data.Add("Vectra", "Chevrolet");
            data.Add("S10", "Chevrolet");

            data.Add("Fit", "Honda");
            data.Add("Civic", "Honda");

            //beige
            //silver
            //red
            //black
            //white

            predictions.ForEach(p =>
            {
                if (data.ContainsKey(p.tagName))
                {
                    p.brand = data[p.tagName];
                    p.model = p.tagName;
                    p.isModel = true;

                    p.tagName = $"{data[p.tagName]} {p.tagName}";
                }
            });
        }

    }
}
