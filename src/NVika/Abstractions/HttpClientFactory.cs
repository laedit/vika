using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace NVika.Abstractions
{
    [ExcludeFromCodeCoverage]
    [Export(typeof(IHttpClientFactory))]
    internal class HttpClientFactory : IHttpClientFactory
    {
        public HttpClient Create()
        {
            return new HttpClient();
        }
    }
}
