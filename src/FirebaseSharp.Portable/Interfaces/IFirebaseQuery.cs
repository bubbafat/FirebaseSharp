using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FirebaseSharp.Portable.Interfaces
{
    public delegate void SnapshotCallback(IDataSnapshot snap, string previousChild = null, object context = null);

    public interface IFirebaseQuery : IFirebaseReadonlyQuery
    {
        IFirebaseReadonlyQuery On(string eventName, SnapshotCallback callback);
        IFirebaseReadonlyQuery On(string eventName, SnapshotCallback callback, object context);
        IFirebaseReadonlyQuery Once(string eventName, SnapshotCallback callback, FirebaseStatusCallback cancelledCallback = null);
        IFirebaseReadonlyQuery Once(string eventName, SnapshotCallback callback, object context, FirebaseStatusCallback cancelledCallback = null);

        IFirebaseQuery OrderByChild(string key);
        IFirebaseQuery OrderByKey();
        IFirebaseQuery OrderByValue<T>();
        IFirebaseQuery OrderByPriority();
        IFirebaseQuery StartAt(string startingValue);
        IFirebaseQuery StartAt(long startingValue);
        IFirebaseQuery EndAt(string endingValue);
        IFirebaseQuery EndAt(long endingValue);
        IFirebaseQuery EqualTo(string value);
        IFirebaseQuery EqualTo(long value);
        IFirebaseQuery LimitToFirst(int limit);
        IFirebaseQuery LimitToLast(int limit);
    }
}
