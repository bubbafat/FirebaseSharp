using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FirebaseSharp.Portable.Network
{
    class FirebaseHttpClient : IFirebaseHttpClient
    {
        private readonly HttpClient _client;

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
                Timeout = TimeSpan.FromMinutes(2),
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
            var response = await _client.SendAsync(request, httpCompletionOption, cancellationToken)
                                        .ConfigureAwait(false);
            return new FirebaseFirebaseHttpResponseMessage(response);
        }

        public void Dispose()
        {
            using(_client) { };
        }
    }
}
