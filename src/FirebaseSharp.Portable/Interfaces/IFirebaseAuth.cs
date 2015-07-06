using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace FirebaseSharp.Portable.Interfaces
{
    public class AuthenticationState
    {
        public string Uid { get; private set; }
        public string Auth { get; private set; }
        public DateTimeOffset Expires { get; private set; }
        public string Provider { get; private set; }
        public string Token { get; private set; }

    }
    public class AuthChangedEventArgs : EventArgs
    {
        public AuthenticationState State { get; private set; }
        public Exception Error { get; private set; }
        public string ErrorCode { get; private set; }
    }

    public delegate void AuthChangedEvent(object sender, AuthChangedEventArgs e);

    public interface IFirebaseAuth
    {
        void AuthWithCustomToken(string authToken);
        void AuthAnonymously();
        void AuthWithPassword(string email, string password);
        void AuthWithOAuthToken(string provider, string credentials);
        AuthenticationState GetAuth();
        void Unauth();

        event AuthChangedEvent AuthChanged;
    }
}
