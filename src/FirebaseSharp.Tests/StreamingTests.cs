using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using FakeItEasy;
using FirebaseSharp.Portable;
using FirebaseSharp.Portable.Network;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirebaseSharp.Tests
{
    [TestClass]
    public class StreamingTests
    {
        [TestMethod]
        public void StreamingEventsFired()
        {
            Uri root = new Uri("http://example.com/root");
            string childPath = "/item/path";
            Uri expectedUri = new Uri("http://example.com/root/item/path.json");

            var client = A.Fake<IFirebaseHttpClient>();
            A.CallTo(() => client.BaseAddress).Returns(root);

            var response = A.Fake<IFirebaseHttpResponseMessage>();

            var responses = new[]
            {
                "event: put",
                "data: {'path': '/value/zero', 'data': {'value': '0'}}",
                "event: patch",
                "data: {'path': '/value/zero', 'data': {'value': 'zero'}}",
                "event: patch",
                "data: {'path': '/value/zero', 'data': null}",
            };

            using (Stream stream = BuildResponseStream(responses))
            {
                A.CallTo(() => response.ReadAsStreamAsync()).Returns(stream);

                A.CallTo(() => client.SendAsync(
                    A<HttpRequestMessage>.That.Matches(
                        (req) => req.MatchStreaming(HttpMethod.Get, expectedUri, "text/event-stream")),
                    A<HttpCompletionOption>.Ignored,
                    A<CancellationToken>.Ignored)).Returns(response);

                Request firebaseRequest = new Request(client, null);
                var result = firebaseRequest.GetStreaming(
                    path: childPath,
                    cancellationToken: CancellationToken.None).Result;

                var addedCallback = A.Fake<ValueAddedEventHandler>();
                var changedCallback = A.Fake<ValueChangedEventHandler>();
                var removedCallback = A.Fake<ValueRemovedEventHandler>();

                result.Added += addedCallback;
                result.Changed += changedCallback;
                result.Removed += removedCallback;

                ManualResetEvent closed = new ManualResetEvent(false);
                result.Closed += (sender, e) => { closed.Set(); };

                result.Listen();

                closed.WaitOne();

                A.CallTo(() => addedCallback.Invoke(A<object>._, A<ValueAddedEventArgs>._)).MustHaveHappened();
                A.CallTo(() => changedCallback.Invoke(A<object>._, A<ValueChangedEventArgs>._)).MustHaveHappened();
                A.CallTo(() => removedCallback.Invoke(A<object>._, A<ValueRemovedEventArgs>._)).MustHaveHappened();
            }
        }
        public static Stream BuildResponseStream(IEnumerable<string> input)
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter sw = new StreamWriter(stream, Encoding.UTF8, 4096, true))
            {
                foreach (string line in input)
                {
                    sw.WriteLine(line);
                }
            }

            stream.Position = 0;

            return stream;
        }
    }
}
