using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using BertaBot.Infrastructure;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BertaBot.CustomVision
{
    public class LocalHttpClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<LocalHttpClient> _logger;

        public readonly string _microsoftAppId;
        public readonly string _microsoftAppPassword;

        public LocalHttpClient(HttpClient client, ILogger<LocalHttpClient> logger, IConfiguration configuration)
        {
            _logger = logger;

            _client = client;

            _microsoftAppId = configuration["MicrosoftAppId"];
            _microsoftAppPassword = configuration["MicrosoftAppPassword"];
        }

        internal async Task<byte[]> GetImageAsync(Attachment attachment, Activity activity = null, bool isSkype = false)
        {
            if (activity != null)
            {
                using (var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl)))
                {
                    var uri = new Uri(attachment.ContentUrl);
                    using (HttpRequestMessage rq = new HttpRequestMessage(HttpMethod.Get, uri))
                    {
                        if (isSkype)
                        {
                            var credentials = connectorClient.Credentials as MicrosoftAppCredentials;
                            credentials.MicrosoftAppId = _microsoftAppId;
                            credentials.MicrosoftAppPassword = _microsoftAppPassword;

                            var token = await credentials.GetTokenAsync();

                            rq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                            rq.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
                        }
                        else
                        {
                            //rq.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(attachment.ContentType));
                        }

                        using (var response = await _client.SendAsync(rq))
                        {
                            return StreamToByteArray(await response.Content.ReadAsStreamAsync());
                        }
                    }
                }
            }
            else
            {
                using (var stream = await _client.GetStreamAsync(attachment.ContentUrl))
                {
                    return StreamToByteArray(stream);
                }
            }
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
    }
}
