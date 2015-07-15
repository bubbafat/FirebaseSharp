using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FirebaseSharp.Portable.Network
{
    class FirebaseResponseMessage : IFirebaseResponseMessage
    {
        private readonly HttpResponseMessage _message;
        public FirebaseResponseMessage(HttpResponseMessage message)
        {
            _message = message;
        }
        public void EnsureSuccessStatusCode()
        {
            _message.EnsureSuccessStatusCode();
        }

        public Task<Stream> ReadAsStreamAsync()
        {
            return _message.Content.ReadAsStreamAsync();
        }

        public Task<string> ReadAsStringAsync()
        {
            return _message.Content.ReadAsStringAsync();
        }

        public void Dispose()
        {
            using (_message) {  }
        }
    }
}
