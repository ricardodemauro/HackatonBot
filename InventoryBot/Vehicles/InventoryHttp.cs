using BertaBot.Vehicles.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BertaBot.Vehicles
{
    public class InventoryHttp
    {
        private readonly HttpClient _client;
        private readonly string _inventoryUri;

        public InventoryHttp(HttpClient client, IConfiguration configuration)
        {
            _client = client;
            _inventoryUri = configuration[""]
        }

        public async Task AddVehicle(CarModel carModel, CancellationToken cancellationToken = default)
        {
            using (var rq = new HttpRequestMessage(HttpMethod.Post, _predictionImageFile))
            {
                rq.Content = new ByteArrayContent(imageBytes);
                rq.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                var response = await _client.SendAsync(rq);
                _logger.LogInformation($"prediction response = {response.StatusCode}");

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("Status code different from ok {StatusCode}", response.StatusCode);
                }

                PredictionRoot result =
                    JsonConvert.DeserializeObject<PredictionRoot>(await response.Content.ReadAsStringAsync());

                return result;
            }
        }
    }
}
