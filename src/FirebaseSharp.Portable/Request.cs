using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FirebaseSharp.Portable
{
    internal sealed class Request : IDisposable
    {
        private readonly HttpClient _client;
        private readonly string _authToken;

        public Request(Uri rootUri, string authToken)
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

            _authToken = authToken;
        }

        public Uri RootUri
        {
            get { return _client.BaseAddress; }
        }

        public async Task<Response> GetStreaming(string path, 
            ValueAddedEventHandler added = null,
            ValueChangedEventHandler changed = null,
            ValueRemovedEventHandler removed = null)
        {
            Uri uri = BuildPath(path);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

            HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            return new Response(response, added, changed, removed);
        }

        internal async Task<string> GetSingle(string path)
        {
            HttpResponseMessage response = await Query(HttpMethod.Get, path);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        internal async Task Delete(string path)
        {
            HttpResponseMessage response = await Query(HttpMethod.Delete, path);

            response.EnsureSuccessStatusCode();
        }

        internal async Task<string> Patch(string path, string payload)
        {
            HttpResponseMessage response = await Query(new HttpMethod("PATCH"), path, payload);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        internal async Task<string> Put(string path, string payload)
        {
            HttpResponseMessage response = await Query(HttpMethod.Put, path, payload);
            
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        internal async Task<string> Post(string path, string payload)
        {
            HttpResponseMessage response = await Query(HttpMethod.Post, path, payload);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        private Task<HttpResponseMessage> Query(HttpMethod method, string path, string payload = null)
        {
            Uri uri = BuildPath(path);

            HttpRequestMessage request = new HttpRequestMessage(method, uri);

            if (!string.IsNullOrEmpty(payload))
            {
                request.Content = new StringContent(payload);
            }

            return _client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
        }

        private Uri BuildPath(string path)
        {
            string uri = RootUri.AbsoluteUri + path + ".json";
            if (!string.IsNullOrEmpty(_authToken))
            {
                uri += string.Format("?auth={0}", _authToken);
            }

            return new Uri(uri);
        }


        public void Dispose()
        {
            using (_client) { }
        }
    }
}
