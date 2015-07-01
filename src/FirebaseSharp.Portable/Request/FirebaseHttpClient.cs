using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Response;
using FirebaseSharp.Portable.Utilities;

namespace FirebaseSharp.Portable.Request
{
    class FirebaseHttpClient : IFirebaseHttpClient
    {
        private readonly HttpClient _client;
        private readonly AsyncLock _clientMutex = new AsyncLock();

        public FirebaseHttpClient(Uri rootUri)
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 10,
            };

            _client = new HttpClient(handler, true)
            {
                BaseAddress = rootUri,
                Timeout = Timeout.InfiniteTimeSpan,
            };

        }

        public Uri BaseAddress
        {
            get { return _client.BaseAddress; }
        }

        public async Task<IFirebaseHttpResponseMessage> SendAsync(HttpRequestMessage request, 
            HttpCompletionOption httpCompletionOption,
            CancellationToken cancellationToken)
        {
            using (await _clientMutex.LockAsync())
            {
                var response = await _client.SendAsync(request, httpCompletionOption, cancellationToken)
                    .ConfigureAwait(false);

                return new FirebaseHttpResponseMessage(response);
            }
        }

        public void Dispose()
        {
            using(_client) { }
        }
    }
}
