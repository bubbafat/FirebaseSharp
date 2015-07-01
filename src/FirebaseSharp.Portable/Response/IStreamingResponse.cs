using System;
using FirebaseSharp.Portable.Cache;
using FirebaseSharp.Portable.Response.Events;

namespace FirebaseSharp.Portable.Response
{
    public interface IStreamingResponse : IDisposable
    {
        event ValueAddedEventHandler Added;
        event ValueChangedEventHandler Changed;
        event ValueRemovedEventHandler Removed;
        event AuthenticationRevokedHandler Revoked;
        event PathCanceledHandler Canceled;

        event StreamingResponseClosedHandler Closed;
        event StreamingResponseIdleTimeoutHandler Timeout;
        event StreamingResponseErrorHandler Error;

        void Listen();
    }
}
