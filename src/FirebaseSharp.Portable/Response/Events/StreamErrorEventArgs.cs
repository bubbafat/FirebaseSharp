using System;

namespace FirebaseSharp.Portable.Response.Events
{
    public class StreamErrorEventArgs : EventArgs
    {
        public StreamErrorEventArgs(Exception ex)
        {
            Error = ex;
        }

        public Exception Error { get; private set; }
    }

    public delegate void StreamingErrorHandler(object sender, StreamErrorEventArgs e);
}
