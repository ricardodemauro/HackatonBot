using System.Net.Http;
using System.Threading.Tasks;
using BertaBot.Bots;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Calling;
using Microsoft.Bot.Builder.Integration.AspNet.Core;

namespace BertaBot.Controllers
{
    // This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    // implementation at runtime. Multiple different IBot implementations running at different endpoints can be
    // achieved by specifying a more specific type for the bot constructor argument.
    [Route("api/calling")]
    [ApiController]
    public class CallController : Controller
    {
        public CallController() : base()
        {
            CallingConversation.RegisterCallingBot(callingBotService => new ConversationBot(callingBotService));
        }

        [Route("callback")]
        public async Task<IActionResult> ProcessCallingEventAsync()
        {
            return await CallingConversation.SendAsync(Request, CallRequestType.CallingEvent);
        }

        [Route("call")]
        public async Task<IActionResult> ProcessIncomingCallAsync()
        {
            return await CallingConversation.SendAsync(Request, CallRequestType.IncomingCall);
        }
    }
}
