using System;

namespace FirebaseSharp.Portable
{
    public class StreamResponseErrorEventArgs : EventArgs
    {
        public StreamResponseErrorEventArgs(Exception ex)
        {
            Error = ex;
        }

        public Exception Error { get; private set; }
    }

    public delegate void StreamResponseErrorHandler(object sender, StreamResponseErrorEventArgs e);
}
