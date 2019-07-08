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
    }


    public class UserProfileDialog : ComponentDialog
    {
        private readonly IStatePropertyAccessor<UserProfile> _userProfileAccessor;
        private IConfiguration _config;
        public UserProfileDialog(UserState userState, IConfiguration config)
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
        }



        private static async Task<DialogTurnResult> ActionStep(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.SendTypingAsync(1500, cancellationToken);
            var cardOptions = new List<Choice>()
            {
                new Choice() { Value = "Add a new car", Synonyms = new List<string>() { "add", "new", "create", "save" } },
                new Choice() { Value = "See inventory", Synonyms = new List<string>() { "list", "inventory", "cars" } },
                new Choice() { Value = "Exit", Synonyms = new List<string>() { "exit", "leave", "quit", "stop" } },
            };

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.
            // Running a prompt here means the next WaterfallStep will be run when the users response is received.
            //https://unicode.org/emoji/charts/full-emoji-list.html#1f596

            await stepContext.SendTypingAsync("Hello my name is Jhon. The newest Cox Automotive team member! \U0001F609", 1000, cancellationToken);
            await stepContext.SendTypingAsync("I'm still an intern, please be patient \U0001F607.", 1000, cancellationToken);

            return await stepContext.PromptAsync(nameof(ChoicePrompt),
                new PromptOptions
                {
                    Prompt = MessageFactory.Text("what would you like to do today?"),
                    Choices = cardOptions,
                }, cancellationToken);
        }

        private static async Task<DialogTurnResult> ActionOptionSelected(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string value = ((FoundChoice)stepContext.Result).Value;

            switch (value)
            {
                case "Add a new car":
                    return await stepContext.PromptAsync(nameof(AttachmentPrompt),
                        new PromptOptions
                        {
                            Prompt = MessageFactory.Text("All right. Could ya pls share an image of the car you wanna add?")
                        }
                        , cancellationToken);
                case "See inventory":
                    await stepContext.Context.SendActivityAsync("nao tem carro!", cancellationToken: cancellationToken);
                    break;
                case "Exit":
                    return await stepContext.EndDialogAsync(new List<string>(), cancellationToken);
                default:
                    await stepContext.Context.SendActivityAsync("ok then, bye!", cancellationToken: cancellationToken);
                    return await stepContext.EndDialogAsync(new List<string>(), cancellationToken);
            }
            await stepContext.Context.SendActivityAsync("nao entendi, vai ter que comecar tudo de novo! ainda estamos melhorando isso", cancellationToken: cancellationToken);
            return await stepContext.EndDialogAsync(new List<string>(), cancellationToken);
            //return await stepContext.PromptAsync(nameof(AttachmentPrompt), new PromptOptions { Prompt = MessageFactory.Text("Please enter your name.") }, cancellationToken);
        }

        private async Task<DialogTurnResult> GetCarImageAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //stepContext.Values["name"] = (string)stepContext.Result;
            await stepContext.SendTypingAsync("Nice car! Hold on...\U0001F3C3", 1000, cancellationToken);

            var image = (List<Attachment>)stepContext.Result;
            string json = await MakePredictionRequest(image[0].ContentUrl, stepContext, cancellationToken);

            PredictionRoot asd = JsonConvert.DeserializeObject<PredictionRoot>(json);


            asd.predictions = asd.predictions.OrderByDescending(x => x.probability).ToList();

            SetVehicleBrandColor(asd.predictions);

            var vehicle = asd.predictions.FirstOrDefault(p => p.isModel);

            //var color = asd.predictions.FirstOrDefault(p => !p.isModel);

            await stepContext.SendTypingAsync(100, cancellationToken);

            List<string> su = new List<string>();
            asd.predictions.ForEach(x =>
            {
                su.Add($"{x.tagName} - {x.probability}, ");
            });

            await stepContext.Context.SendActivityAsync(MessageFactory.SuggestedActions(su), cancellationToken);
            // We can send messages to the user at any point in the WaterfallStep.
            //await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Thanks {stepContext.Result}."), cancellationToken);

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog; here it is a Prompt Dialog.

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text($"\U0001F575 Well well wel. Look what we've got here. A {vehicle.tagName} \U0001F440, right?")
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
                    Prompt = MessageFactory.Text("Pois diga aí qual é o correto \U0001F44D"),
                    //RetryPrompt = MessageFactory.Text("The value entered must be greater than 0 and less than 150."),
                };

                return await stepContext.PromptAsync(nameof(TextPrompt), promptOptions, cancellationToken);
            }
            else
            {
                await stepContext.SendTypingAsync(MessageFactory.ContentUrl("https://media.giphy.com/media/ckeHl52mNtoq87veET/giphy.gif", "image/gif"), 500, cancellationToken);
                await stepContext.SendTypingAsync("I knew it!", 1000, cancellationToken);
                await stepContext.SendTypingAsync("I knew it!", 1000, cancellationToken);

                // User said "no" so we will skip the next step. Give -1 as the age.
                return await stepContext.NextAsync(-1, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["age"] = (int)stepContext.Result;

            var msg = (int)stepContext.Values["age"] == -1 ? "No age given." : $"I have your age as {stepContext.Values["age"]}.";

            // We can send messages to the user at any point in the WaterfallStep.
            await stepContext.Context.SendActivityAsync(MessageFactory.Text(msg), cancellationToken);

            // WaterfallStep always finishes with the end of the Waterfall or with another dialog, here it is a Prompt Dialog.
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions { Prompt = MessageFactory.Text("Is this ok?") }, cancellationToken);
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

        public async Task<string> MakePredictionRequest(string imageUri, WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var client = new HttpClient();

            byte[] imageBytes;
            using (var stream = await client.GetStreamAsync(imageUri))
            {
                imageBytes = StreamToByteArray(stream);
            }

            client.DefaultRequestHeaders.Add("Prediction-Key", _config["predictionKey"]);

            await stepContext.SendTypingAsync("Just one more sec...", 1000, cancellationToken: cancellationToken);
            string jsonResult = string.Empty;
            using (var content = new ByteArrayContent(imageBytes))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                string url = _config["predictionBaseUri"] + "/" + _config["predictionUrl"];
                var response = await client.PostAsync(url, content);
                jsonResult = await response.Content.ReadAsStringAsync();
            }

            await stepContext.SendTypingAsync("Done. Too many details on my registration form here.", 1000, cancellationToken: cancellationToken);
            await stepContext.SendTypingAsync("Would be nice to have some kind of \U0001F916 to do all this paper work, don't you think?", 1000, cancellationToken: cancellationToken);
            return jsonResult;
        }

        private static byte[] GetImageAsByteArray(string imageFilePath)
        {
            FileStream fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            BinaryReader binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
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
