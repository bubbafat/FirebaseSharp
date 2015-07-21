namespace FirebaseSharp.Portable.Interfaces
{
    public interface IFirebaseOnDisconnect
    {
        void Set(string value, FirebaseStatusCallback callback = null);
        void Update(string value, FirebaseStatusCallback callback = null);
        void Remove(FirebaseStatusCallback callback = null);
        void SetPriority(string value, string priority, FirebaseStatusCallback callback = null);
        void SetPriority(string value, float priority, FirebaseStatusCallback callback = null);
        void Cancel();
    }
}