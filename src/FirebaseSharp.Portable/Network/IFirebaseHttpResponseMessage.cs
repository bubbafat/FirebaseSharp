using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirebaseSharp.Portable.Network
{
    interface IFirebaseHttpResponseMessage : IDisposable
    {
        void EnsureSuccessStatusCode();

        Task<Stream> ReadAsStreamAsync();

        Task<string> ReadAsStringAsync();
    }
}
