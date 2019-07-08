using BertaBot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace BertaBot.CustomVision
{
    public class CogServicesHttp
    {
        private readonly HttpClient _client;
        private readonly ILogger<CogServicesHttp> _logger;
        private readonly string _predictionImageFile;
        private readonly string _predictionImageUrl;

        public CogServicesHttp(HttpClient client, IConfiguration configuration, ILogger<CogServicesHttp> logger)
        {
            _logger = logger;

            _client = client;
            //_client.BaseAddress = new Uri(configuration["predictionBaseUri"]);
            _client.DefaultRequestHeaders.Add("Prediction-Key", configuration["predictionKey"]);

            _predictionImageFile = configuration["predictionImageFile"];
            _predictionImageUrl = configuration["predictionImageUrl"];
        }

        internal async Task<PredictionRoot> MakePredictionAsync(byte[] imageBytes)
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

        internal async Task<PredictionRoot> MakePredictionUriAsync(string imageUri)
        {
            using (var rq = new HttpRequestMessage(HttpMethod.Post, _predictionImageUrl))
            {
                var rqObj = new PredictionUriRequest
                {
                    Url = imageUri
                };

                rq.Content = new StringContent(JsonConvert.SerializeObject(rqObj));
                rq.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                var response = await _client.SendAsync(rq);

                _logger.LogInformation("prediction response {Status}", response.StatusCode);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    _logger.LogError("Status code different from ok {StatusCode}", response.StatusCode);
                }

                PredictionRoot result = JsonConvert.DeserializeObject<PredictionRoot>(await response.Content.ReadAsStringAsync());

                return result;
            }
        }
    }
}
