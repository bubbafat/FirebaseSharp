using System;
using System.IO;
using System.Threading.Tasks;

namespace FirebaseSharp.Portable.Response
{
    interface IFirebaseHttpResponseMessage : IDisposable
    {
        void EnsureSuccessStatusCode();

        Task<Stream> ReadAsStreamAsync();

        Task<string> ReadAsStringAsync();
    }
}
