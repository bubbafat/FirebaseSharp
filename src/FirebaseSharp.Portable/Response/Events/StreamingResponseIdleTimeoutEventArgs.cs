using System;

namespace FirebaseSharp.Portable.Response.Events
{
    public class StreamingResponseIdleTimeoutEventArgs : EventArgs
    {
        public StreamingResponseIdleTimeoutEventArgs()
        {
        }
    }

    public delegate void StreamingResponseIdleTimeoutHandler(object sender, StreamingResponseIdleTimeoutEventArgs e);

}
