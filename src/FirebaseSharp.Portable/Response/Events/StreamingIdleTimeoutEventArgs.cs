using System;

namespace FirebaseSharp.Portable.Response.Events
{
    public class StreamingIdleTimeoutEventArgs : EventArgs
    {
        public StreamingIdleTimeoutEventArgs()
        {
        }
    }

    public delegate void StreamingIdleTimeoutHandler(object sender, StreamingIdleTimeoutEventArgs e);

}
