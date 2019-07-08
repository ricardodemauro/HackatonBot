using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace BertaBot.Infrastructure
{
    public static class HttpClientBuilderExtensions
    {
        public static IHttpClientBuilder WithProxy(this IHttpClientBuilder httpClientBuilder, bool bypassSslCheck = true)
        {
            httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler();

                if (bypassSslCheck)
                    handler.ServerCertificateCustomValidationCallback = (message, certificate2, chain, errors) => true;

                return handler;
            });

            return httpClientBuilder;
        }
    }
}
