using System;

namespace FirebaseSharp.Portable.Response.Events
{
    internal class StreamingDataReceviedEventArgs : EventArgs
    {
        public StreamingDataReceviedEventArgs(StreamingEvent eventData)
        {
            Event = eventData;
        }

        public StreamingEvent Event { get; private set; }
    }

    internal delegate void StreamingDataReceivedHandler(object sender, StreamingDataReceviedEventArgs e);
}
