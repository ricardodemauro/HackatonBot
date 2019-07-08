// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Microsoft.BotBuilderSamples
{
    public static class Extensions
    {
        public static async Task SendTypingAsync(this WaterfallStepContext stepContext, int time, CancellationToken cancellationToken)
        {
            var typing = new Activity(ActivityTypes.Typing);
            await stepContext.Context.SendActivityAsync(typing, cancellationToken: cancellationToken);
            Thread.Sleep(time);
        }

        public static async Task SendTypingAsync(this WaterfallStepContext stepContext, IMessageActivity activity, int time, CancellationToken cancellationToken)
        {
            var typing = new Activity(ActivityTypes.Typing);
            await stepContext.Context.SendActivityAsync(typing, cancellationToken: cancellationToken);
            Thread.Sleep(time);

            await stepContext.Context.SendActivityAsync(activity, cancellationToken: cancellationToken);
        }

        public static async Task SendTypingAsync(this WaterfallStepContext stepContext, string message, int time, CancellationToken cancellationToken)
        {
            var typing = new Activity(ActivityTypes.Typing);
            await stepContext.Context.SendActivityAsync(typing, cancellationToken: cancellationToken);
            Thread.Sleep(time);

            await stepContext.Context.SendActivityAsync(message, cancellationToken: cancellationToken);
        }

        public static async Task<DialogTurnResult> PromptTypingAsync(this WaterfallStepContext stepContext, string dialogId, PromptOptions options, int time, CancellationToken cancellationToken = default)
        {
            var typing = new Activity(ActivityTypes.Typing);
            await stepContext.Context.SendActivityAsync(typing, cancellationToken: cancellationToken);
            Thread.Sleep(time);
            return await stepContext.PromptAsync(dialogId, options, cancellationToken);
        }
    }



    public class PredictionRoot
    {
        public string id { get; set; }
        public string project { get; set; }
        public string iteration { get; set; }
        public DateTime created { get; set; }
        public List<Prediction> predictions { get; set; }
    }

    public class Prediction
    {
        public decimal probability { get; set; }
        public string tagId { get; set; }
        public string tagName { get; set; }
        public bool isModel { get; set; }

        public override string ToString()
        {
            return tagName;
        }
    }


    public class BotSettings
    {
        public string Name { get; set; }
        public string WelcomeMessage { get; set; }
        public string InternWarning { get; set; }
        public string WhatToDo { get; set; }
        public string AddCar { get; set; }
        public List<string> AddCarSynonyms { get; set; }
        public string SeeInventory { get; set; }
        public List<string> SeeInventorySynonyms { get; set; }
        public string Exit { get; set; }
        public List<string> ExitSynonyms { get; set; }
        public string AskForImages { get; set; }
        public string CarsReceived { get; set; }
        public string Processing { get; set; }
        public string ProcessingFinished { get; set; }
        public string Complain { get; set; }
        public string CheckAnswer { get; set; }
        public string CorrectAnwer { get; set; }
        public string WrongAnswer { get; set; }
    }



    public class UserProfileDialog : ComponentDialog
    {

        private static BotSettings _settings;
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;
        private readonly IConfiguration _config;

        private static ILogger _logger;

        public UserProfileDialog(UserState userState, IConfiguration config, ILogger<UserProfileDialog> logger)
            : base(nameof(UserProfileDialog))
        {
            _config = config;
            _userProfileAccessor = userState.CreateProperty<UserProfile>("UserProfile");

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
            _settings = JsonConvert.DeserializeObject<BotSettings>(_config["botSettings"]);
            _logger = logger;
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

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the users response is received.
            //https://unicode.org/emoji/charts/full-emoji-list.html#1f596

            await stepContext.SendTypingAsync(_settings.WelcomeMessage, 1000, cancellationToken);
            await stepContext.SendTypingAsync(_settings.InternWarning, 1000, cancellationToken);

            _logger.LogInformation("Display options");

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

            _logger.LogInformation($"selected option = {value}");

            switch (value)
            {
                case "Add a new car":
                    _logger.LogInformation("ask for images");
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

                    _logger.LogInformation("display available cars");

                    await stepContext.SendTypingAsync(MessageFactory.Carousel(attachments), 1000, cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(new List<string>(), cancellationToken);
                case "Exit":
                    _logger.LogInformation("exiting");
                    await stepContext.Context.SendActivityAsync("ok then, bye!", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(new List<string>(), cancellationToken);
                default:
                    _logger.LogInformation("invalid option");
                    await stepContext.Context.SendActivityAsync("nao entendi, vai ter que comecar tudo de novo! ainda estamos melhorando isso", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(new List<string>(), cancellationToken);
            }
        }

        private async Task<DialogTurnResult> GetCarImageAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //stepContext.Values["name"] = (string)stepContext.Result;
            await stepContext.SendTypingAsync(_settings.CarsReceived, 1000, cancellationToken);

            var images = (List<Attachment>)stepContext.Result;

            _logger.LogInformation($"{images.Count} images received");
            List<Prediction> predictions = await MakePredictionRequest(images.Select(x => x.ContentUrl).ToList(), stepContext, cancellationToken);

            _logger.LogInformation($"ordering predictions");
            predictions = predictions.OrderByDescending(x => x.probability).ToList();

            SetVehicleBrandColor(predictions);

            _logger.LogInformation($"get first prediction item");
            var vehicle = predictions.FirstOrDefault(p => p.isModel);

            //var color = asd.predictions.FirstOrDefault(p => !p.isModel);

            await stepContext.SendTypingAsync(100, cancellationToken);

            //List<string> su = new List<string>();
            //asd.predictions.ForEach(x =>
            //{
            //    su.Add($"{x.tagName} - {x.probability}, ");
            //});

            //await stepContext.Context.SendActivityAsync(MessageFactory.SuggestedActions(su), cancellationToken);
            // We can send messages to the user at any point in the WaterfallStep.
            //await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks {stepContext.Result}."), cancellationToken);

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.

            _logger.LogInformation($"ask user to confirm the vehicle name prediction");
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

                _logger.LogInformation($"wrong prediction, ask for input");
                return await stepContext.PromptTypingAsync(nameof(TextPrompt), promptOptions, 1000, cancellationToken);
            }
            else
            {
                _logger.LogInformation($"correct prediction");
                await stepContext.SendTypingAsync(MessageFactory.ContentUrl("https://media.giphy.com/media/ckeHl52mNtoq87veET/giphy.gif", "image/gif"), 500, cancellationToken);
                await stepContext.SendTypingAsync(_settings.CorrectAnwer, 1000, cancellationToken);
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
            //var msg = (int)stepContext.Values["age"] == -1 ? "No age given." : $"I have your age as {stepContext.Values["age"]}.";

            //// We can send messages to the user at any point in the WaterfallStep.
            //await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            //// WaterfallStep always finishes with the end of the Waterfall or with another dialog, here it is a Prompt Dialog.
            //return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Is this ok?") }, cancellationToken);
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

        static byte[] StreamToByteArray(Stream stream)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public async Task<List<Prediction>> MakePredictionRequest(List<string> imageUris, WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            List<Prediction> predictions = new List<Prediction>();

            foreach (var imageUri in imageUris)
            {
                var client = new HttpClient();

                byte[] imageBytes;
                using (var stream = await client.GetStreamAsync(imageUri))
                {
                    imageBytes = StreamToByteArray(stream);
                }

                client.DefaultRequestHeaders.Add("Prediction-Key", _config["predictionKey"]);

                await stepContext.SendTypingAsync(_settings.Processing.Replace("#NUMBER", (imageUris.IndexOf(imageUri) + 1).ToString()), 1500, cancellationToken: cancellationToken);
                string jsonResult = string.Empty;
                using (var content = new ByteArrayContent(imageBytes))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    string url = _config["predictionBaseUri"] + "/" + _config["predictionUrl"];
                    var response = await client.PostAsync(url, content);
                    _logger.LogInformation($"prediction response = {response.StatusCode}");
                    PredictionRoot result = JsonConvert.DeserializeObject<PredictionRoot>(await response.Content.ReadAsStringAsync());
                    predictions.AddRange(result.predictions.OrderByDescending(x => x.probability).Take(4));
                }
            }

            await stepContext.SendTypingAsync(_settings.ProcessingFinished, 1000, cancellationToken: cancellationToken);
            await stepContext.SendTypingAsync(_settings.Complain, 1000, cancellationToken: cancellationToken);

            return predictions;
        }

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
                    p.tagName = $"{data[p.tagName]} {p.tagName}";
                    p.isModel = true;
                }
            });
        }

    }
}
