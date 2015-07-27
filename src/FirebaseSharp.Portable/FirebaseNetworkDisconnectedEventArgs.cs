using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FirebaseSharp.Portable
{
    class FirebaseNetworkDisconnectedEventArgs : EventArgs
    {
        public FirebaseNetworkDisconnectedEventArgs(string reason, Exception ex = null)
        {
            Reason = reason;
            Error = ex;
        }

        public string Reason { get; private set; }
        public Exception Error { get; private set; }
    }
}
