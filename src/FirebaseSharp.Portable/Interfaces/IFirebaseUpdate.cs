namespace FirebaseSharp.Portable.Interfaces
{
    public interface IFirebaseUpdate
    {
        void Set(string value, FirebaseStatusCallback callback = null);
        void Set(object value, FirebaseStatusCallback callback = null);
        void Update(string value, FirebaseStatusCallback callback = null);
        void Remove(FirebaseStatusCallback callback = null);
        IFirebase Push(string value = null, FirebaseStatusCallback callback = null);
        IFirebase Push(object value, FirebaseStatusCallback callback = null);
        void SetWithPriority(string value, string priority, FirebaseStatusCallback callback = null);
        void SetWithPriority(string value, float priority, FirebaseStatusCallback callback = null);
        void SetPriority(string priority, FirebaseStatusCallback callback = null);
        void SetPriority(float priority, FirebaseStatusCallback callback = null);
    }
}