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

        Task<IFirebaseHttpResponseMessage> SendAsync(
            System.Net.Http.HttpRequestMessage request, 
            System.Net.Http.HttpCompletionOption httpCompletionOption, 
            System.Threading.CancellationToken cancellationToken);
    }
}
