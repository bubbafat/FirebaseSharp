using System;

namespace FirebaseSharp.Portable
{
    public interface IFirebaseApp : IDisposable
    {
        void GoOnline();
        void GoOffline();
        IFirebase Child(string path);
    }
}