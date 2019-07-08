using BertaBot.Vehicles.Models;
using Microsoft.Extensions.Configuration;
using BertaBot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using BertaBot.Infrastructure;

namespace BertaBot.Vehicles
{
    public class InventoryHttp
    {
        private readonly HttpClient _client;
        private readonly string _inventoryUri;
        private readonly ILogger<InventoryHttp> _logger;

        public InventoryHttp(HttpClient client, IConfiguration configuration, ILogger<InventoryHttp> logger)
        {
            _client = client;
            _inventoryUri = configuration["inventoryUrl"];
            _logger = logger;
        }

        static string GetContentType(string type)
        {
            return "data:image/jpeg;base64,";
        }

        public async Task<bool> AddVehicle(CarModel carModel, IList<(byte[], string)> images = default, CancellationToken cancellationToken = default)
        {
            if (images != null && images.Count > 0)
            {
                foreach (var imageItem in images)
                {
                    var baseImage = Base64Encoder.ToBase64(imageItem.Item1);
                    var contentImage = string.Concat(GetContentType(imageItem.Item2), baseImage);
                    carModel.Base64Images.Add(contentImage);
                }
            }

            using (var rq = new HttpRequestMessage(HttpMethod.Post, _inventoryUri))
            {
                rq.Content = new StringContent(JsonConvert.SerializeObject(carModel));
                rq.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                try
                {
                    var response = await _client.SendAsync(rq, cancellationToken);

                    _logger.LogInformation($"prediction response = {response.StatusCode}");

                    if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Created)
                    {
                        return true;
                    }
                    _logger.LogError("Status code different from ok {StatusCode}", response.StatusCode);

                    return false;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "something went wrong when sending to inventory");
                    throw;
                }

            }
        }
    }
}
