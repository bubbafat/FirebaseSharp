namespace FirebaseSharp.Portable.Interfaces
{
    public interface IFirebaseReadonlyQuery
    {
        void Off();
        IFirebaseQuery Ref();
    }
}