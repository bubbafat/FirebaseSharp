using System.Collections.Generic;

namespace FirebaseSharp.Portable
{
    public interface IDataSnapshot
    {
        bool Exists { get; }
        IDataSnapshot Child(string childName);
        IEnumerable<IDataSnapshot> Children { get; }
        bool HasChildren { get; }
        int NumChildren { get; }
        IFirebase Ref();
        FirebasePriority GetPriority();
        string Key { get; }
        string ExportVal();
        T Value<T>();
        string Value();

        IDataSnapshot this[string child] { get; }
    }
}