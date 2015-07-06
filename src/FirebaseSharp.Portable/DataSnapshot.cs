using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Interfaces;

namespace FirebaseSharp.Portable
{
    class DataSnapshot : IDataSnapshot
    {
        public bool Exists { get; private set; }
        public IDataSnapshot Child(string path)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IDataSnapshot> Children { get; private set; }
        public bool HasChildren { get; private set; }
        public int NumChildren { get; private set; }
        public IFirebase Ref { get; private set; }
        public IFirebasePriority Priority { get; private set; }
        public string Key { get; private set; }
        public string Value { get; private set; }
        public string ExportVal { get; private set; }
    }
}
