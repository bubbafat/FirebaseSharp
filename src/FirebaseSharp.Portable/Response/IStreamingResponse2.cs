using System.Threading.Tasks;
using FirebaseSharp.Portable.Response.Events;

namespace FirebaseSharp.Portable.Response
{
    interface IStreamingResponse2
    {
        Task Listen();

        event StreamingDataReceivedHandler Received;
        event StreamingErrorHandler Error;
        event StreamingClosedHandler Closed;
        event StreamingIdleTimeoutHandler Timeout;
    }
}
