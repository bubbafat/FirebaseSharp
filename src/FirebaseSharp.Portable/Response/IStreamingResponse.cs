using System;

namespace FirebaseSharp.Portable
{
    public interface IStreamingResponse : IDisposable
    {
        event ValueAddedEventHandler Added;
        event ValueChangedEventHandler Changed;
        event ValueRemovedEventHandler Removed;
        event AuthenticationRevokedHandler Revoked;
        event StreamingResponseClosedHandler Closed;
        event PathCanceledHandler Canceled;

        void Listen();
    }
}
