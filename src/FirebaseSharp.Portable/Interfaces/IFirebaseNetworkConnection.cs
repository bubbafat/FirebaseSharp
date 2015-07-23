using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Messages;

namespace FirebaseSharp.Portable.Interfaces
{
    internal class FirebaseEventReceivedEventArgs : EventArgs
    {
        public FirebaseEventReceivedEventArgs(FirebaseMessage message)
        {
            Message = message;
        }

        public FirebaseMessage Message { get; private set; }
    }

    internal interface IFirebaseNetworkConnection : IDisposable
    {
        void Send(FirebaseMessage message);

        event EventHandler<FirebaseEventReceivedEventArgs> Received;

        void Disconnect();
        void Connect();
    }
}
