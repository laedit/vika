using NVika.Abstractions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NVika.Tests.Mocks
{
    internal class HttpClientFactoryMock : IHttpClientFactory
    {
        public HttpMessageHandlerMock HttpMessageHandler { get; private set; }

        public HttpClientFactoryMock(HttpStatusCode responseStatusCode)
        {
            HttpMessageHandler = new HttpMessageHandlerMock(new HttpResponseMessage(responseStatusCode));
        }

        public void SetResponse(HttpResponseMessage response)
        {
            HttpMessageHandler = new HttpMessageHandlerMock(response);
        }

        public HttpClient Create()
        {
            return new HttpClient(HttpMessageHandler);
        }

        public class HttpMessageHandlerMock : HttpMessageHandler
        {
            private HttpResponseMessage _response;

            public List<Tuple<HttpRequestMessage, string>> Requests { get; private set; }

            public HttpMessageHandlerMock(HttpResponseMessage response)
            {
                _response = response;
                Requests = new List<Tuple<HttpRequestMessage, string>>();
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Requests.Add(new Tuple<HttpRequestMessage, string>(request, request.Content.ReadAsStringAsync().Result));
                return Task.FromResult(_response);
            }
        }
    }
}
