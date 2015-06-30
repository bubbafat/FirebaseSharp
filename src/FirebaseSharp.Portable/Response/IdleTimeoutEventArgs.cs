using System;

namespace FirebaseSharp.Portable
{
    public class IdleTimeoutEventArgs : EventArgs
    {
        public IdleTimeoutEventArgs()
        {
        }
    }

    public delegate void IdleTimeoutHandler(object sender, IdleTimeoutEventArgs e);

}
