using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FirebaseSharp.Portable.Network
{
    class FirebaseFirebaseHttpResponseMessage : IFirebaseHttpResponseMessage
    {
        private readonly HttpResponseMessage _response;
        public FirebaseFirebaseHttpResponseMessage(HttpResponseMessage response)
        {
            _response = response;
        }

        public void EnsureSuccessStatusCode()
        {
            _response.EnsureSuccessStatusCode();
        }

        public async Task<Stream> ReadAsStreamAsync()
        {
            return await _response.Content.ReadAsStreamAsync();
        }

        public async Task<string> ReadAsStringAsync()
        {
            return await _response.Content.ReadAsStringAsync();
        }

        public void Dispose()
        {
            using (_response) { }
        }
    }
}
