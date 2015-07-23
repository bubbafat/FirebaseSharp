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
        public FirebaseMessage(WriteBehavior behavior, FirebasePath path, string value, FirebaseStatusCallback callback)
            : this(behavior, path, value, null, callback)
        {
        }

        public FirebaseMessage(WriteBehavior behavior, FirebasePath path, string value, FirebasePriority priority, FirebaseStatusCallback callback)
        {
            Behavior = behavior;
            Path = path;
            Value = value;
            Callback = callback;
            Priority = priority;
        }
        public WriteBehavior Behavior {get; private set;}
        public FirebasePath Path { get; private set; }
        public string Value { get; private set; }
        public FirebaseStatusCallback Callback { get; private set; }
        public FirebasePriority Priority { get; private set; }
    }
}
