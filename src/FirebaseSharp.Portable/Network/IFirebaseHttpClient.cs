using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FirebaseSharp.Portable.Network
{
    interface IFirebaseHttpClient : IDisposable
    {
        Uri BaseAddress { get; }

        Task<IFirebaseHttpResponseMessage> SendAsync(
            HttpRequestMessage request, 
            HttpCompletionOption httpCompletionOption, 
            CancellationToken cancellationToken);
    }
}
