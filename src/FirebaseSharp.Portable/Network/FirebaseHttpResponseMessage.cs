using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FirebaseSharp.Portable.Network
{
    class FirebaseHttpResponseMessage : IFirebaseHttpResponseMessage
    {
        private readonly HttpResponseMessage _response;

        public FirebaseHttpResponseMessage(HttpResponseMessage response)
        {
            _response = response;
        }

        public void EnsureSuccessStatusCode()
        {
            _response.EnsureSuccessStatusCode();
        }

        public async Task<Stream> ReadAsStreamAsync()
        {
            return await _response.Content
                                .ReadAsStreamAsync()
                                .WithTimeout(Config.NetworkReadTimeout)
                                .ConfigureAwait(false);
        }

        public async Task<string> ReadAsStringAsync()
        {
            return await _response.Content
                                .ReadAsStringAsync()
                                .WithTimeout(Config.NetworkReadTimeout)
                                .ConfigureAwait(false);
        }

        public void Dispose()
        {
            using (_response) { }
        }
    }
}
