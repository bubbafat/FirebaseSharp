using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace FirebaseSharp.Portable
{
    internal sealed class Request : IDisposable
    {
        private readonly HttpClient _client;
        private readonly string _authToken;
        private readonly JsonCache _cache = new JsonCache();
        private readonly Task _sendTask;

        public Request(Uri rootUri, string authToken)
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 10,
            };

            _client = new HttpClient(handler, true)
            {
                BaseAddress = rootUri,
                Timeout = TimeSpan.FromMinutes(120),
            };

            _cache.Changed += CacheOnChanged;

            _authToken = authToken;

            _sendTask = Task.Run(() =>
            {
                DrainQueue().Wait();
            });
        }

        readonly BlockingQueue<DataChangedEventArgs> _unsentLocals = new BlockingQueue<DataChangedEventArgs>();

        private async Task DrainQueue()
        {
            int waitMs = 10;
            TimeSpan maxWait = TimeSpan.FromMinutes(1);

            while (true)
            {
                var next = _unsentLocals.Dequeue();
                try
                {
                    var response = await Query(new HttpMethod(next.HttpMethod.ToString()), next.Path, next.Data).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    waitMs = 10;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ERROR: {0}", ex.Message);
                    _unsentLocals.Reque(next);
                    Task.Delay(waitMs).Wait();
                    waitMs = Math.Min(waitMs * 2, (int)maxWait.TotalMilliseconds);
                }
            }
        }

        private async void CacheOnChanged(object sender, DataChangedEventArgs e)
        {
            if (e.Source == ChangeSource.Local)
            {
                _unsentLocals.Enqueue(e);
            }
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

            HttpResponseMessage response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return new Response(response, _cache, added, changed, removed);
        }

        internal async Task<string> GetSingle(string path)
        {
            HttpResponseMessage response = await Query(HttpMethod.Get, path).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        internal void Delete(string path)
        {
            _cache.Put(ChangeSource.Local, path, null);
        }

        internal void Patch(string path, string payload)
        {
            _cache.Patch(ChangeSource.Local, path, payload);
        }

        internal void Put(string path, string payload)
        {
            _cache.Put(ChangeSource.Local, path, payload);
        }

        internal async Task<string> Post(string path, string payload)
        {
            HttpResponseMessage response = await Query(HttpMethod.Post, path, payload).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        private async Task<HttpResponseMessage> Query(HttpMethod method, string path, string payload = null)
        {
            Uri uri = BuildPath(path);

            HttpRequestMessage request = new HttpRequestMessage(method, uri);

            if (!string.IsNullOrEmpty(payload))
            {
                request.Content = new StringContent(payload);
            }

            Debug.WriteLineIf(request.Content == null, 
                string.Format("{0} {1}", method.Method, path));

            Debug.WriteLineIf(request.Content != null,
                string.Format("{0} {1}: {2}", method.Method, path, payload));

            return await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);
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
