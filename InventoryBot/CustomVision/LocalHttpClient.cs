using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BertaBot.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BertaBot.CustomVision
{
    public class LocalHttpClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<LocalHttpClient> _logger;

        public LocalHttpClient(HttpClient client, ILogger<LocalHttpClient> logger)
        {
            _logger = logger;

            _client = client;
        }

        internal async Task<byte[]> GetImageAsync(string uri)
        {
            using (var stream = await _client.GetStreamAsync(uri))
            {
                return StreamToByteArray(stream);
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
