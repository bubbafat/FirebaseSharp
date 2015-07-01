using System;

namespace FirebaseSharp.Portable.Response.Events
{
    public class StreamingResponseClosedEventArgs : EventArgs
    {
        public StreamingResponseClosedEventArgs()
        {
        }
    }

    public delegate void StreamingResponseClosedHandler(object sender, StreamingResponseClosedEventArgs e);
}