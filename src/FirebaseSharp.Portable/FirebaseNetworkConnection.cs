using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable
{
    class FirebaseNetworkConnection : IFirebaseNetworkConnection
    {
        private bool _connected = false;
        private HttpClient _client;
        private readonly Uri _root;
        private readonly BlockingQueue<FirebaseMessage> _sendQueue = new BlockingQueue<FirebaseMessage>();
        private readonly Task _sendTask;
        private readonly Task _receiveTask;
        public FirebaseNetworkConnection(Uri root)
        {
            _root = root;

            _sendTask = Task.Run(() => SendThread());
            _receiveTask = Task.Run(() => ReceiveThread());

            Connect();
        }

        private async void SendThread()
        {
            while (true)
            {
                var message = _sendQueue.Dequeue();
                HttpRequestMessage request = new HttpRequestMessage(GetMethod(message), GetUri(message.Path));
                if (!string.IsNullOrEmpty(message.Value))
                {
                    request.Content = new StringContent(message.Value);
                } 

                await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead).ContinueWith(
                    (rsp) =>
                    {
                        if (rsp.IsFaulted)
                        {
                            message.Callback(new FirebaseError(rsp.Exception.Message));
                        }
                    }, TaskContinuationOptions.NotOnRanToCompletion);
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

        private async void ReceiveThread()
        {
            while (true)
            {
                var uri = GetUri("/");
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

                var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync())
                using (var reader = new StreamReader(stream))
                {
                    WriteBehavior behavior = WriteBehavior.None;
                    string data = null;

                    while (true)
                    {

                        var line = reader.ReadLine();
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
                                    data = null;
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
                            data = line.Substring(5).Trim();
                            OnReceived(behavior, data);

                            behavior = WriteBehavior.None;
                            data = null;
                        }
                    }
                }   
            }
        }

        private void OnReceived(WriteBehavior behavior, string data)
        {
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

        public void Send(FirebaseMessage message)
        {
            _sendQueue.Enqueue(message);
        }

        public event FirebaseEventReceived Received;
        public void Disconnect()
        {
            _connected = false;
        }

        public void Connect()
        {
            _connected = true;
            HttpClientHandler handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 10,
            };

            _client = new HttpClient(handler, true)
            {
                BaseAddress = _root,
                Timeout = TimeSpan.FromMinutes(15),
            };
        }
    }
}
