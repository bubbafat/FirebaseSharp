using System;

namespace FirebaseSharp.Portable.Interfaces
{
    public interface IFirebasePriority : IComparable<IFirebasePriority>
    {
        void FromJson(string json);
        string ToJson();
    }
}