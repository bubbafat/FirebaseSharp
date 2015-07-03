using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable
{
    public sealed class Response : IDisposable
    {
        private readonly CancellationTokenSource _cancel;
        private readonly Task _pollingTask;
        private readonly JsonCache _cache;

        internal Response(HttpResponseMessage response,
            ValueAddedEventHandler added = null,
            ValueChangedEventHandler changed = null,
            ValueRemovedEventHandler removed = null)
        {
            _cancel = new CancellationTokenSource();

            _cache = new JsonCache();
            _cache.Changed += OnCacheChanged;

            _pollingTask = ReadLoop(response, _cancel.Token);

            if (added != null)
            {
                Added += added;
            }
            if (changed != null)
            {
                Changed += changed;
            }
            if (removed != null)
            {
                Removed += removed;
            }
        }

        public event ValueAddedEventHandler Added;
        public event ValueChangedEventHandler Changed;
        public event ValueRemovedEventHandler Removed;

        private void OnCacheChanged(object sender, DataChangedEventArgs e)
        {
            switch (e.Event)
            {
                case EventType.Added:
                    var added = Added;
                    if (added != null)
                    {
                        added(this, new ValueAddedEventArgs(e.Path, e.Data));
                    }
                    break;
                case EventType.Changed:
                    var changed = Changed;
                    if (changed != null)
                    {
                        changed(this, new ValueChangedEventArgs(e.Path, e.Data, e.OldData));
                    }
                    break;
                case EventType.Removed:
                    var removed = Removed;
                    if (removed != null)
                    {
                        removed(this, new ValueRemovedEventArgs(e.Path));
                    }
                    break;
            }
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
                    JObject jo = JObject.Parse(p);
                    string path = jo["path"].ToString();

                    var dataObj = jo["data"];
                    string data = (dataObj.Type == JTokenType.Null) ? null : dataObj.ToString();
                    
                    if (eventName == "put")
                    {
                        _cache.Put(ChangeSource.Remote, path, data);
                    }
                    else
                    {
                        _cache.Patch(ChangeSource.Remote, path, data);
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
