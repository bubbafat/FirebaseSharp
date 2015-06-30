using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Network;

namespace FirebaseSharp.Portable
{
    internal sealed class Request : IDisposable
    {
        private readonly IFirebaseHttpClient _client;
        private readonly string _authToken;

        public Request(IFirebaseHttpClient client, string authToken)
        {
            _client = client;
            _authToken = authToken;
        }

        public Uri RootUri
        {
            get { return _client.BaseAddress; }
        }

        public async Task<IStreamingResponse> GetStreaming(string path, CancellationToken cancellationToken)
        {
            Uri uri = BuildPath(path);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

            IFirebaseHttpResponseMessage response;

            response = await _client.SendAsync(request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return new StreamingResponse(response, cancellationToken);
        }

        internal async Task<string> GetSingle(string path, CancellationToken cancellationToken)
        {
            IFirebaseHttpResponseMessage response = await Query(HttpMethod.Get, path, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return await response.ReadAsStringAsync().ConfigureAwait(false);
        }

        internal async Task Delete(string path, CancellationToken cancellationToken)
        {
            IFirebaseHttpResponseMessage response = await Query(HttpMethod.Delete, path, cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
        }

        internal async Task<string> Patch(string path, string payload, CancellationToken cancellationToken)
        {
            IFirebaseHttpResponseMessage response = await Query(new HttpMethod("PATCH"), path, payload, cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return await response.ReadAsStringAsync().ConfigureAwait(false);
        }

        internal async Task<string> Put(string path, string payload, CancellationToken cancellationToken)
        {
            IFirebaseHttpResponseMessage response = await Query(HttpMethod.Put, path, payload, cancellationToken).ConfigureAwait(false);
            
            response.EnsureSuccessStatusCode();

            return await response.ReadAsStringAsync().ConfigureAwait(false);
        }

        internal async Task<string> Post(string path, string payload, CancellationToken cancellationToken)
        {
            IFirebaseHttpResponseMessage response = await Query(HttpMethod.Post, path, payload, cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return await response.ReadAsStringAsync().ConfigureAwait(false);
        }

        private async Task<IFirebaseHttpResponseMessage> Query(HttpMethod method, string path, CancellationToken cancellationToken)
        {
            return await Query(method, path, null, cancellationToken).ConfigureAwait(false);
        }

        private async Task<IFirebaseHttpResponseMessage> Query(HttpMethod method, string path, string payload,
            CancellationToken cancellationToken)
        {
            Debug.WriteLine("SEND (path): {0}", path);
            Uri uri = BuildPath(path);

            HttpRequestMessage request = new HttpRequestMessage(method, uri);

            if (!string.IsNullOrEmpty(payload))
            {
                Debug.WriteLine("SEND (payload): {0}", payload);
                request.Content = new StringContent(payload);
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            IFirebaseHttpResponseMessage response;

            try
            {
                response = await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }

            sw.Stop();
            Debug.WriteLine("SEND (timing): {0} ms", sw.ElapsedMilliseconds);

            return response;
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
