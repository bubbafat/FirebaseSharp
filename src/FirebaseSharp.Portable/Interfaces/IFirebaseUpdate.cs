using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FirebaseSharp.Portable.Interfaces
{
    public delegate string TransactionUpdate(string currentData);

    public delegate void TransactionComplete(FirebaseError error, bool committed, IDataSnapshot dataSnapshot);

    public interface IFirebaseUpdate
    {
        void Set(string value, FirebaseStatusCallback callback = null);
        void Update(string value, FirebaseStatusCallback callback = null);
        void Remove(FirebaseStatusCallback callback = null);
        IFirebase Push(string value = null, FirebaseStatusCallback callback = null);
        void SetWithPriority(string value, FirebasePriority priority, FirebaseStatusCallback callback = null);
        void SetPriority(FirebasePriority priority, FirebaseStatusCallback callback = null);
        void Transaction(TransactionUpdate updateCallback, TransactionComplete completeCallback = null, bool applyLocally = true);
    }
}
