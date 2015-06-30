using System;

namespace FirebaseSharp.Portable
{
    public class AuthenticationRevokedEventArgs : EventArgs
    {
        public AuthenticationRevokedEventArgs()
        {
        }
    }

    public delegate void AuthenticationRevokedHandler(object sender, AuthenticationRevokedEventArgs e);
}
