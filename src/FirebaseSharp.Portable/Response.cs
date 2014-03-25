using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FirebaseSharp.Portable
{
    public sealed class Response : IDisposable
    {
        private readonly HttpResponseMessage _response;
        private readonly CancellationTokenSource _cancel;
        private readonly Task _pollingTask;

        internal Response(HttpResponseMessage response, Action<StreamingEvent> callback)
        {
            _response = response;

            _cancel = new CancellationTokenSource();

            _pollingTask = ReadLoop(_response, callback, _cancel.Token);
        }

        public void Cancel()
        {
            _cancel.Cancel();
        }

        public void Wait()
        {
            Wait(TimeSpan.MaxValue);
        }
        public void Wait(TimeSpan timeout)
        {
            _pollingTask.Wait(timeout);
        }

        private static async Task ReadLoop(HttpResponseMessage response, Action<StreamingEvent> callback, CancellationToken cancellationToken)
        {
            using (var content = await response.Content.ReadAsStreamAsync())
            using (StreamReader sr = new StreamReader(content))
            {
                string eventName = null;

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string read = await sr.ReadLineAsync();
                    if (read.StartsWith("event: "))
                    {
                        eventName = read.Substring(7);
                        continue;
                    }

                    if (read.StartsWith("data: "))
                    {
                        if (string.IsNullOrEmpty(eventName))
                        {
                            throw new InvalidOperationException("Payload data was received but an event did not preceed it.");
                        }

                        callback(new StreamingEvent(eventName, read.Substring(6)));
                    }

                    // start over
                    eventName = null;
                }
            }
        }
        public void Dispose()
        {
            using (_response) { }
            using (_cancel) { }
        }
    }
}
