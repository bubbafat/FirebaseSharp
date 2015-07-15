using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace FirebaseSharp.Portable.Network
{
    class FirebaseHttpClient : IFirebaseHttpClient
    {
        private readonly HttpClient _client;

        public FirebaseHttpClient(Uri baseUri)
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 10,
            };

            _client = new HttpClient(handler, true)
            {
                BaseAddress = baseUri,
                Timeout = TimeSpan.FromMinutes(15),
            };

        }
        public Uri BaseAddress
        {
            get { return _client.BaseAddress; }
        }
        public async Task<IFirebaseResponseMessage> SendAsync(HttpRequestMessage message, HttpCompletionOption options)
        {
            var response = await _client.SendAsync(message, options);
            return new FirebaseResponseMessage(response);
        }

        public void Dispose()
        {
            using (_client) { }
        }
    }
}
