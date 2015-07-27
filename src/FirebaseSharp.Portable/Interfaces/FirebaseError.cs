namespace FirebaseSharp.Portable.Interfaces
{
    public delegate void FirebaseStatusCallback(FirebaseError error);

    public class FirebaseError
    {
        public FirebaseError(string code)
        {
            Code = code;
        }

        public string Code { get; private set; }
    }
}