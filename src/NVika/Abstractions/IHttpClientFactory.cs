using System.Net.Http;

namespace NVika.Abstractions
{
    internal interface IHttpClientFactory
    {
        HttpClient Create();
    }
}