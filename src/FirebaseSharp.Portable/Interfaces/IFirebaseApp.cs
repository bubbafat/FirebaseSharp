using System;

namespace FirebaseSharp.Portable.Interfaces
{
    public interface IFirebaseApp : IDisposable
    {
        void GoOnline();
        void GoOffline();
        IFirebase Child(string path);
    }
}
