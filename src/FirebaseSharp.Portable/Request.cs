using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FirebaseSharp.Portable
{
    internal sealed class Request : IDisposable
    {
        private readonly HttpClient _client;

        public Request(Uri rootUri)
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
            };

            _client = new HttpClient(handler, true)
            {
                BaseAddress = rootUri,
                Timeout = TimeSpan.FromMinutes(1),
            };
        }

        public Uri RootUri
        {
            get { return _client.BaseAddress; }
        }

        public async Task<Response> GetStreaming(Uri path, Action<StreamingEvent> callback)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

            HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            return new Response(response, callback);
        }

        internal async Task<string> GetSingle(Uri path)
        {
            HttpResponseMessage response = await Query(HttpMethod.Get, path);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        internal async Task Delete(Uri path)
        {
            HttpResponseMessage response = await Query(HttpMethod.Delete, path);

            response.EnsureSuccessStatusCode();
        }

        internal async Task Patch(Uri path, string payload)
        {
            HttpResponseMessage response = await Query(new HttpMethod("PATCH"), path, payload);

            response.EnsureSuccessStatusCode();
        }

        internal async Task<string> Put(Uri path, string payload)
        {
            HttpResponseMessage response = await Query(HttpMethod.Post, path, payload);
            
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        internal async Task<string> Post(Uri path, string payload)
        {
            HttpResponseMessage response = await Query(HttpMethod.Post, path, payload);

            dynamic result = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());

            response.EnsureSuccessStatusCode();

            return result.name;
        }

        private Task<HttpResponseMessage> Query(HttpMethod method, Uri path, string payload = null,
            string acceptType = null)
        {
            HttpRequestMessage request = new HttpRequestMessage(method, path);

            if (!string.IsNullOrEmpty(payload))
            {
                request.Content = new StringContent(payload);
            }

            if (acceptType != null)
            {
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptType));
            }

            return _client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
        }

        public void Dispose()
        {
            using (_client) { }
        }
    }
}
