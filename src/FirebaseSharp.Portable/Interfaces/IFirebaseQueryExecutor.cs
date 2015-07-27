namespace FirebaseSharp.Portable.Interfaces
{
    public delegate void SnapshotCallback(IDataSnapshot snap, string previousChild = null, object context = null);

    public interface IFirebaseQueryExecutor
    {
        IFirebaseReadonlyQuery On(string eventName, SnapshotCallback callback);
        IFirebaseReadonlyQuery On(string eventName, SnapshotCallback callback, object context);

        IFirebaseReadonlyQuery Once(string eventName, SnapshotCallback callback,
            FirebaseStatusCallback cancelledCallback = null);

        IFirebaseReadonlyQuery Once(string eventName, SnapshotCallback callback, object context,
            FirebaseStatusCallback cancelledCallback = null);
    }
}