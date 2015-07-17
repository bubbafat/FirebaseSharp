using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Interfaces;

namespace FirebaseSharp.Portable.Messages
{
    enum WriteBehavior
    {
        None,
        Replace,
        Merge,
        Push,
    }

    class FirebaseMessage
    {
        public FirebaseMessage(WriteBehavior behavior, string path, string value, FirebaseStatusCallback callback)
        {
            Behavior = behavior;
            Path = path;
            Value = value;
            Callback = callback;
        }
        public WriteBehavior Behavior {get; private set;}
        public string Path { get; private set; }
        public string Value { get; private set; }
        public FirebaseStatusCallback Callback { get; private set; }
    }
}
