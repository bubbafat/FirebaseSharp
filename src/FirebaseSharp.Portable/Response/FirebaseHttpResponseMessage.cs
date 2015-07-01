using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Utilities;

namespace FirebaseSharp.Portable.Response
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

        public async Task<Stream> ReadAsStreamAsync(CancellationToken cancellationToken)
        {
            return await _response.Content
                                .ReadAsStreamAsync()
                                .WithTimeout(Config.NetworkReadTimeout, cancellationToken)
                                .ConfigureAwait(false);
        }

        public async Task<string> ReadAsStringAsync(CancellationToken cancellationToken)
        {
            return await _response.Content
                                .ReadAsStringAsync()
                                .WithTimeout(Config.NetworkReadTimeout, cancellationToken)
                                .ConfigureAwait(false);
        }

        public void Dispose()
        {
            using (_response) { }
        }
    }
}
