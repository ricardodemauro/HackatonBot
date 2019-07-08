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
    public static class BotExtensions
    {
        public static async Task SendTypingAsync(this WaterfallStepContext stepContext, int time, CancellationToken cancellationToken)
        {
            var typing = new Activity(ActivityTypes.Typing);
            await stepContext.Context.SendActivityAsync(typing, cancellationToken: cancellationToken);
        }

        public static async Task SendTypingAsync(this WaterfallStepContext stepContext, IMessageActivity activity, int time, CancellationToken cancellationToken)
        {
            var typing = new Activity(ActivityTypes.Typing);
            await stepContext.Context.SendActivityAsync(typing, cancellationToken: cancellationToken);

            await stepContext.Context.SendActivityAsync(activity, cancellationToken: cancellationToken);
        }

        public static async Task SendTypingAsync(this WaterfallStepContext stepContext, string message, int time, CancellationToken cancellationToken)
        {
            var typing = new Activity(ActivityTypes.Typing);
            await stepContext.Context.SendActivityAsync(typing, cancellationToken: cancellationToken);

            await stepContext.Context.SendActivityAsync(message, cancellationToken: cancellationToken);
        }

        public static async Task<DialogTurnResult> PromptTypingAsync(this WaterfallStepContext stepContext, string dialogId, PromptOptions options, int time, CancellationToken cancellationToken = default)
        {
            var typing = new Activity(ActivityTypes.Typing);
            await stepContext.Context.SendActivityAsync(typing, cancellationToken: cancellationToken);
            return await stepContext.PromptAsync(dialogId, options, cancellationToken);
        }
    }
}
