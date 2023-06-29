using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;

namespace TenantManagement.Services
{
    public abstract class NotifyBase
    {
        private readonly IHttpClientFactory _clientFactory;

        public NotifyBase(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<HttpResponseMessage> PostApiCall(StringContent content, string apiEndPoint)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, apiEndPoint)
            {
                Content = content
            };

            request.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json);

            var client = _clientFactory.CreateClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

            var response = await client.SendAsync(request);
            return response;
        }
    }
}