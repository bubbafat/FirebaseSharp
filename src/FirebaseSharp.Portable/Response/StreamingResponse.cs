using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FirebaseSharp.Portable
{
    public sealed class StreamingResponse : IDisposable
    {
        // used to keep a reference to the task around
        private Task _pollingTask;
        private readonly FirebaseCache _cache;

        private readonly CancellationTokenSource _localCancelSource = new CancellationTokenSource();
        private CancellationTokenSource _mixedCancelSource;

        internal StreamingResponse()
        {
            _cache = new FirebaseCache();
        }

        public void Listen(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            _mixedCancelSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken,
                _localCancelSource.Token);

            _pollingTask =
                Task.Run(() => { ReadLoop(response, _mixedCancelSource.Token).Wait(_mixedCancelSource.Token); },
                    _mixedCancelSource.Token);
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

        private void OnRevoked(string message)
        {
            var revoked = Revoked;
            if (revoked != null)
            {
                revoked(this, new AuthenticationRevokedEventArgs(message));
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

        private async Task ReadLoop(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            using (response)
            using (var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using (StreamReader sr = new StreamReader(content))
            {
                string eventName = null;

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // TODO: it really sucks that this does not take a cancellation token
                    string read = await sr.ReadLineAsync().ConfigureAwait(false);

                    // for whatever reason we're at the end of the stream
                    // this can happen when auth is revoked or ... ?
                    if (read == null)
                    {
                        System.Diagnostics.Debug.WriteLine("RECV: <end of stream>");
                        _mixedCancelSource.Cancel();
                        OnClosed();
                        return;
                    }
                    
                    System.Diagnostics.Debug.WriteLine("RECV: {0}", read);

                    if (read.StartsWith("event: "))
                    {
                        eventName = read.Substring(7);
                        continue;
                    }

                    if (read.StartsWith("data: "))
                    {
                        if (string.IsNullOrEmpty(eventName))
                        {
                            throw new InvalidOperationException(
                                "Payload data was received but an event did not preceed it.");
                        }

                        Update(eventName, read.Substring(6));
                    }

                    // start over
                    eventName = null;
                }
            }
        }

        private void Update(string eventName, string p)
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

                        break;
                    case "auth_revoked":
                        _mixedCancelSource.Cancel();
                        OnRevoked("revoked");
                        break;
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
            _mixedCancelSource.Cancel();

            using (_mixedCancelSource) { }
            using (_localCancelSource) { }
        }
    }
}
