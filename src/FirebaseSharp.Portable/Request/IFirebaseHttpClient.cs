using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Response;

namespace FirebaseSharp.Portable.Request
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
