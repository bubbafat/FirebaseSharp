using System;

namespace FirebaseSharp.Portable
{
    public class StreamingResponseIdleTimeoutEventArgs : EventArgs
    {
        public StreamingResponseIdleTimeoutEventArgs()
        {
        }
    }

    public delegate void StreamingResponseIdleTimeoutHandler(object sender, StreamingResponseIdleTimeoutEventArgs e);

}
