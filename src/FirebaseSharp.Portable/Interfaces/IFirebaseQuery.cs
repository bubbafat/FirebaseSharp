using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FirebaseSharp.Portable.Interfaces
{
    public delegate void SnapshotCallback(IDataSnapshot snap, string previousChild = null, object context = null);

    public interface IFirebaseQuery
    {
        void On(string eventName, SnapshotCallback callback);
        void On(string eventName, object context, SnapshotCallback callback);
        void Off(string eventName, SnapshotCallback callback);
        void Off(string eventName, object context, SnapshotCallback callback);
        void Once(string eventName, SnapshotCallback callback, FirebaseStatusCallback cancelledCallback = null);
        void Once(string eventName, object context, SnapshotCallback callback, FirebaseStatusCallback cancelledCallback = null);
        IFirebase OrderByChild(string key);
        IFirebase OrderByKey();
        IFirebase OrderByValue();
        IFirebase OrderByPriority();
        IFirebase StartAt(object startingValue);
        IFirebase EndAt(object startingValue);
        IFirebase equalTo(object value);
        IFirebase limitToFirst(int limit);
        IFirebase limitToLast(int limit);
        IFirebase Ref();
    }
}
