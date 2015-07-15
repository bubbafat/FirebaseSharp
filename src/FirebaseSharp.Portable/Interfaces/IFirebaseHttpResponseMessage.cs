using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FirebaseSharp.Portable.Interfaces
{
    interface IFirebaseHttpResponseMessage : IDisposable
    {
        void EnsureSuccessStatusCode();

        Task<Stream> ReadAsStreamAsync(CancellationToken cancellationToken);

        Task<string> ReadAsStringAsync(CancellationToken cancellationToken);
    }
}
