using System;

namespace FirebaseSharp.Portable
{
    public class AuthenticationRevokedEventArgs : EventArgs
    {
        public AuthenticationRevokedEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; private set; }
    }

    public delegate void AuthenticationRevokedHandler(object sender, AuthenticationRevokedEventArgs e);
}
