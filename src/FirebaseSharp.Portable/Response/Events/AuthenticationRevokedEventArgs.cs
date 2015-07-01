using System;

namespace FirebaseSharp.Portable.Response.Events
{
    public class AuthenticationRevokedEventArgs : EventArgs
    {
        public AuthenticationRevokedEventArgs()
        {
        }
    }

    public delegate void AuthenticationRevokedHandler(object sender, AuthenticationRevokedEventArgs e);
}
