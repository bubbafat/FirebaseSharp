using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Network;
using Newtonsoft.Json;

namespace FirebaseSharp.Portable
{
    internal class StreamingResponse : IStreamingResponse
    {
        // used to keep a reference to the task around
        private Task _pollingTask;
        private readonly FirebaseCache _cache;

        private readonly CancellationToken _cancellationToken;
        private readonly IFirebaseHttpResponseMessage _response;

        internal StreamingResponse(IFirebaseHttpResponseMessage response, CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _cache = new FirebaseCache();
            _response = response;
        }

        public void Listen()
        {
            _pollingTask =
                Task.Run(() =>
                {
                    Task loop = ReadLoop(_response, _cancellationToken);
                    loop.ConfigureAwait(false);
                    loop.Wait(_cancellationToken);
                },
                _cancellationToken);
        }

        public event ValueAddedEventHandler Added
        {
            add { _cache.Added += value; }
            remove { _cache.Added -= value; }
        }
        public event ValueChangedEventHandler Changed
        {
            add { _cache.Changed += value; }
            remove { _cache.Changed -= value; }
        }

        public event ValueRemovedEventHandler Removed
        {
            add { _cache.Removed += value; }
            remove { _cache.Removed -= value; }
        }

        public event AuthenticationRevokedHandler Revoked;
        public event StreamingResponseClosedHandler Closed;
        public event PathCanceledHandler Canceled;
        public event StreamingResponseIdleTimeoutHandler Timeout;
        public event StreamResponseErrorHandler Error;

        private void OnIdleTimeout()
        {
            var timeout = Timeout;
            if (timeout != null)
            {
                timeout(this, new StreamingResponseIdleTimeoutEventArgs());
            }
        }

        private void OnCanceled()
        {
            var canceled = Canceled;
            if (canceled != null)
            {
                canceled(this, new PathCanceledEventArgs());
            }
        }

        private void OnRevoked()
        {
            var revoked = Revoked;
            if (revoked != null)
            {
                revoked(this, new AuthenticationRevokedEventArgs());
            }
        }

        private void OnClosed()
        {
            var closed = Closed;
            if (closed != null)
            {
                closed(this, new StreamingResponseClosedEventArgs());
            }
        }

        private void OnError(Exception ex)
        {
            var error = Error;
            if (error != null)
            {
                error(this, new StreamResponseErrorEventArgs(ex));
            }
        }

        private async Task ReadLoop(IFirebaseHttpResponseMessage response, CancellationToken cancellationToken)
        {
            try
            {
                using (var content = await response.ReadAsStreamAsync().ConfigureAwait(false))
                using (StreamReader sr = new StreamReader(content))
                {
                    string eventName = null;

                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        // this can throw TimeoutException
                        string read = await sr.ReadLineAsync()
                                              .WithTimeout(Config.NetworkReadTimeout)
                                              .ConfigureAwait(false);

                        // for whatever reason we're at the end of the stream
                        // this can happen when auth is revoked or ... ?
                        if (read == null)
                        {
                            System.Diagnostics.Debug.WriteLine("RECV: <end of stream>");
                            return;
                        }

                        System.Diagnostics.Debug.WriteLine("RECV: {0}", read);

                        if (read.StartsWith("event:"))
                        {
                            eventName = read.Substring(6).Trim();
                            continue;
                        }

                        if (read.StartsWith("data:"))
                        {
                            if (string.IsNullOrEmpty(eventName))
                            {
                                throw new InvalidOperationException(
                                    "Payload data was received but an event did not preceed it.");
                            }

                            if (!Update(eventName, read.Substring(5).Trim()))
                            {
                                return;
                            }
                        }

                        // start over
                        eventName = null;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // we'll just call Closed
            }
            catch (TimeoutException)
            {
                OnIdleTimeout();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            finally
            {
                OnClosed();                
            }
        }

        private bool Update(string eventName, string p)
        {
            using (JsonReader reader = new JsonTextReader(new StringReader(p)))
            {
                switch (eventName)
                {
                    case "put":
                    case "patch":
                        ReadToNamedPropertyValue(reader, "path");
                        reader.Read();
                        string path = reader.Value.ToString();

                        if (eventName == "put")
                        {
                            _cache.Replace(path, ReadToNamedPropertyValue(reader, "data"));
                        }
                        else
                        {
                            _cache.Update(path, ReadToNamedPropertyValue(reader, "data"));
                        }

                        return true;
                    case "cancel":
                        OnCanceled();
                        return false;
                    case "auth_revoked":
                        OnRevoked();
                        return false;
                    case "keep-alive":
                        return true;
                    default:
#if DEBUG
                        throw new Exception("Unknown event: " + eventName);
#endif
                        return true;
                }
            }
        }

        private JsonReader ReadToNamedPropertyValue(JsonReader reader, string property)
        {
            while (reader.Read() && reader.TokenType != JsonToken.PropertyName)
            {
                // skip the property
            }

            string prop = reader.Value.ToString();
            if (property != prop)
            {
                throw new InvalidOperationException("Error parsing response.  Expected json property named: " + property);
            }

            return reader;
        }

        public void Dispose()
        {
            using (_response) { }
        }
    }
}
