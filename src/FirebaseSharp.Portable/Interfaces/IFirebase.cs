using FirebaseSharp.Portable.Interfaces;

namespace FirebaseSharp.Portable
{
    public interface IFirebase : IFirebaseStructure, IFirebaseUpdate, IFirebaseQueryExecutorAny
    {
        IFirebaseApp GetApp();
    }
}