using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Messages;

namespace FirebaseSharp.Portable
{
    class FirebaseNetworkConnection : IFirebaseNetworkConnection
    {
        private readonly Uri _root;
        public FirebaseNetworkConnection(Uri root)
        {
            _root = root;
        }

        public Task Send(FirebaseMessage message)
        {
            throw new NotImplementedException();
        }

        public event FirebaseEventReceived Received;
        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public void Connect()
        {
            throw new NotImplementedException();
        }
    }
}
