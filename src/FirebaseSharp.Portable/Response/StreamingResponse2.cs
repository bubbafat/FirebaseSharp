using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Response.Events;
using FirebaseSharp.Portable.Utilities;

namespace FirebaseSharp.Portable.Response
{
    internal class StreamingResponse2 : IStreamingResponse2
    {
        private readonly CancellationToken _cancellationToken;
        private readonly IFirebaseHttpResponseMessage _response;

        internal StreamingResponse2(IFirebaseHttpResponseMessage response, CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _response = response;
        }

        public Task Listen()
        {
            return Task.Run(() =>
            {
                Task loop = ReadLoop(_response, _cancellationToken);
                loop.ConfigureAwait(false);
                loop.Wait(_cancellationToken);
            },
            _cancellationToken);
        }

        public event StreamingDataReceivedHandler Received;
        public event StreamingErrorHandler Error;
        public event StreamingClosedHandler Closed;
        public event StreamingIdleTimeoutHandler Timeout;

        private async Task ReadLoop(IFirebaseHttpResponseMessage response, CancellationToken cancellationToken)
        {
            try
            {
                using (var content = await response.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
                using (StreamReader sr = new StreamReader(content))
                {
                    string eventName = null;

                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        // this can throw TimeoutException
                        string read = await sr.ReadLineAsync()
                            .WithTimeout(Config.NetworkReadTimeout, cancellationToken)
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

                            string data = read.Substring(5).Trim();
                            OnReceived(new StreamingEvent(eventName, data));
                        }

                        // start over
                        eventName = null;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // we'll just fire the Closed event
            }
            catch (TimeoutException)
            {
                OnTimeout();
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

        private void OnReceived(StreamingEvent eventData)
        {
            var received = Received;
            if (received != null)
            {
                received(this, new StreamingDataReceviedEventArgs(eventData));
            }
        }

        private void OnError(Exception ex)
        {
            var error = Error;
            if (error != null)
            {
                error(this, new StreamErrorEventArgs(ex));
            }
        }

        private void OnClosed()
        {
            var closed = Closed;
            if (closed != null)
            {
                closed(this, new StreamingClosedEventArgs());
            }
        }

        private void OnTimeout()
        {
            var timeout = Timeout;
            if (timeout != null)
            {
                timeout(this, new StreamingIdleTimeoutEventArgs());
            }
        }
    }
}
