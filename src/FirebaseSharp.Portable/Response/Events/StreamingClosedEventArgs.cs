using System;

namespace FirebaseSharp.Portable.Response.Events
{
    public class StreamingClosedEventArgs : EventArgs
    {
        public StreamingClosedEventArgs()
        {
        }
    }

    public delegate void StreamingClosedHandler(object sender, StreamingClosedEventArgs e);
}