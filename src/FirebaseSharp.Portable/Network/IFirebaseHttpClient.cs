using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FirebaseSharp.Portable.Network
{
    interface IFirebaseHttpClient : IDisposable
    {
        Uri BaseAddress { get; }
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage message, HttpCompletionOption options);
    }
}
