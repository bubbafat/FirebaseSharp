using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Messages;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable
{
    class FirebaseNetworkConnection : IFirebaseNetworkConnection
    {
        private readonly object _lock = new object();
        private bool _connected;
        private HttpClient _client;
        private readonly Uri _root;
        private readonly BlockingQueue<FirebaseMessage> _sendQueue = new BlockingQueue<FirebaseMessage>();
        private CancellationTokenSource _cancelSource;
        private Task _sendTask;
        private Task _receiveTask;
        public FirebaseNetworkConnection(Uri root)
        {
            _root = root;
        }

        private async void SendThread(CancellationToken cancel)
        {
            try
            {
                while (true)
                {
                    cancel.ThrowIfCancellationRequested();

                    var message = _sendQueue.Dequeue(cancel);

                    HttpRequestMessage request = new HttpRequestMessage(GetMethod(message), GetUri(message.Path));
                    
                    if (!string.IsNullOrEmpty(message.Value))
                    {
                        request.Content = new StringContent(message.Value);
                    }

                    await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancel).ContinueWith(
                        (rsp) =>
                        {
                            if (rsp.Exception != null)
                            {
                                message.Callback(new FirebaseError(rsp.Exception.Message));
                            }
                            else
                            {
                                message.Callback(null);
                            }

                        }, TaskContinuationOptions.NotOnCanceled).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private Uri GetUri(string path)
        {
            string uri = _root.AbsoluteUri + path + ".json";

            //if (!string.IsNullOrEmpty(_authToken))
            //{
            //    uri += string.Format("?auth={0}", _authToken);
            //}

            return new Uri(uri, UriKind.Absolute);
        }

        private HttpMethod GetMethod(FirebaseMessage message)
        {
            switch (message.Behavior)
            {
                case WriteBehavior.Merge:
                    return new HttpMethod("PATCH");
                case WriteBehavior.Push:
                    return HttpMethod.Post;
                case WriteBehavior.Replace:
                    return HttpMethod.Put;
                default:
                    throw new NotImplementedException();
            }
        }

        private async void ReceiveThread(CancellationToken cancel)
        {
            try
            {
                while (true)
                {
                    cancel.ThrowIfCancellationRequested();

                    var uri = GetUri("/");
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

                    var response =
                        await
                            _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancel)
                                .ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();

                    using (
                        var stream =
                            await response.Content.ReadAsStreamAsync().WithCancellation(cancel).ConfigureAwait(false))
                    using (var reader = new StreamReader(stream))
                    {
                        WriteBehavior behavior = WriteBehavior.None;

                        while (true)
                        {
                            cancel.ThrowIfCancellationRequested();

                            var line = await reader.ReadLineAsync().WithCancellation(cancel).ConfigureAwait(false);
                            if (line == null)
                            {
                                continue;
                            }

                            Debug.WriteLine("RECV: {0}", line);

                            if (line.StartsWith("event:"))
                            {
                                string eventName = line.Substring(6).Trim();
                                switch (eventName)
                                {
                                    case "cancel":
                                    case "auth_revoked":
                                    case "keep-alive":
                                        OnReceived(WriteBehavior.None, null);
                                        behavior = WriteBehavior.None;
                                        break;
                                    case "put":
                                        behavior = WriteBehavior.Replace;
                                        break;
                                    case "patch":
                                        behavior = WriteBehavior.Merge;
                                        break;
                                }
                            }

                            if (line.StartsWith("data:"))
                            {
                                OnReceived(behavior, line.Substring(5).Trim());

                                behavior = WriteBehavior.None;
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                
            }
        }

        private void OnReceived(WriteBehavior behavior, string data)
        {
            if (behavior == WriteBehavior.None)
            {
                return;
            }

            var callback = Received;
            if (callback != null)
            {
                JObject result = JObject.Parse(data);

                var args =
                    new FirebaseEventReceivedEventArgs(new FirebaseMessage(behavior, 
                        result["path"].ToString(), 
                        result["data"].ToString(), 
                        null));

                callback(this, args);
            }
        }

        public void Send(FirebaseMessage message)
        {
            _sendQueue.Enqueue(_cancelSource.Token, message);
        }

        public event EventHandler<FirebaseEventReceivedEventArgs> Received;
        public void Disconnect()
        {
            lock (_lock)
            {
                if (_connected)
                {
                    _cancelSource.Cancel();
                    _connected = false;
                    _cancelSource = null;
                }
            }
        }

        public void Connect()
        {
            lock (_lock)
            {
                if (!_connected)
                {
                    HttpClientHandler handler = new HttpClientHandler
                    {
                        AllowAutoRedirect = true,
                        MaxAutomaticRedirections = 10,
                    };

                    _client = new HttpClient(handler, true)
                    {
                        BaseAddress = _root,
                        Timeout = TimeSpan.FromMilliseconds(Timeout.Infinite),
                    };

                    _cancelSource = new CancellationTokenSource();
                    _sendTask = Task.Run(() => SendThread(_cancelSource.Token));
                    _receiveTask = Task.Run(() => ReceiveThread(_cancelSource.Token));
                    _connected = true;
                }
            }
        }

        public void Dispose()
        {
            Disconnect();
            using (_client) {  }
            using (_cancelSource) { }
        }
    }
}
