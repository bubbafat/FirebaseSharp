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
        void On(string eventName, SnapshotCallback callback, object context);
        void Off(string eventName, SnapshotCallback callback);
        void Off(string eventName, SnapshotCallback callback, object context);
        void Once(string eventName, SnapshotCallback callback, FirebaseStatusCallback cancelledCallback = null);
        void Once(string eventName, SnapshotCallback callback, object context, FirebaseStatusCallback cancelledCallback = null);
        IFirebase OrderByChild(string key);
        IFirebase OrderByKey();
        IFirebase OrderByValue();
        IFirebase OrderByPriority();
        IFirebase StartAt(string startingValue);
        IFirebase StartAt(long startingValue);
        IFirebase EndAt(string endingValue);
        IFirebase EndAt(long endingValue);
        IFirebase EqualTo(string value);
        IFirebase EqualTo(long value);
        IFirebase LimitToFirst(int limit);
        IFirebase LimitToLast(int limit);
        IFirebase Ref();
    }
}
