using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirebaseSharp.Portable.Interfaces
{
    public interface IDataSnapshot
    {
        bool Exists { get; }
        IDataSnapshot Child(string path);
        IEnumerable<IDataSnapshot> Children { get; }
        bool HasChildren { get; }
        int NumChildren { get; }
        IFirebase Ref { get; }
        IFirebasePriority Priority { get; }
        string Key { get; }
        string Value { get; }
        string ExportVal { get; }
    }
}
