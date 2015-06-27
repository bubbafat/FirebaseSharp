using System;

namespace FirebaseSharp.Portable
{
    public class StreamingResponseClosedEventArgs : EventArgs
    {
        public StreamingResponseClosedEventArgs()
        {
        }
    }

    public delegate void StreamingResponseClosedHandler(object sender, StreamingResponseClosedEventArgs e);
}