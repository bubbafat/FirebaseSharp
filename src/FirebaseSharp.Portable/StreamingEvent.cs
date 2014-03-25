
namespace FirebaseSharp.Portable
{
    public class StreamingEvent
    {
        public StreamingEvent(string eventName, string payload)
        {
            Event = eventName;
            Payload = payload;
        }
        public string Event { get; private set; }
        public string Payload { get; private set; }
    }
}
