using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FirebaseSharp.Portable
{
    public sealed class Response : IDisposable
    {
        private readonly CancellationTokenSource _cancel;
        private readonly Task _pollingTask;
        private readonly FirebaseCache _cache;

        internal Response(HttpResponseMessage response, 
            ValueAddedEventHandler added = null,
            ValueChangedEventHandler changed = null,
            ValueRemovedEventHandler removed = null)
        {
            _cancel = new CancellationTokenSource();

            _cache = new FirebaseCache();

            if(added != null) { _cache.Added += added; }
            if (changed != null) { _cache.Changed += changed; }
            if (removed != null) { _cache.Removed += removed; }

            _pollingTask = ReadLoop(response, _cancel.Token);
        }

        public void Cancel()
        {
            _cancel.Cancel();
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

                    System.Diagnostics.Debug.WriteLine(read);

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
            switch (eventName)
            {
                case "put":
                case "patch":
                    using (StringReader r = new StringReader(p))
                    using(JsonReader reader = new JsonTextReader(r))
                    {
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

                    }
                    break;
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
            Cancel();
            using (_cancel) { }
        }
    }
}
