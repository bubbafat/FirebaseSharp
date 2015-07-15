using System;
using System.IO;
using System.Threading.Tasks;

namespace FirebaseSharp.Portable.Network
{
    interface IFirebaseResponseMessage : IDisposable
    {
        void EnsureSuccessStatusCode();
        Task<Stream> ReadAsStreamAsync();
        Task<string> ReadAsStringAsync();
    }
}
