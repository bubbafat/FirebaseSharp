﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using FakeItEasy;
using FirebaseSharp.Portable;
using FirebaseSharp.Portable.Cache;
using FirebaseSharp.Portable.Request;
using FirebaseSharp.Portable.Response;
using FirebaseSharp.Portable.Response.Events;
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
                        req => req.MatchStreaming(HttpMethod.Get, expectedUri, "text/event-stream")),
                    A<HttpCompletionOption>.Ignored,
                    A<CancellationToken>.Ignored)).Returns(response);

                FirebaseRequest firebaseFirebaseRequest = new FirebaseRequest(client, null);
                var result = firebaseFirebaseRequest.GetStreaming(
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

        [TestMethod]
        public void AuthRevokedFiresRevoked()
        {
            Uri root = new Uri("http://example.com/root");
            string childPath = "/item/path";
            Uri expectedUri = new Uri("http://example.com/root/item/path.json");

            var client = A.Fake<IFirebaseHttpClient>();
            A.CallTo(() => client.BaseAddress).Returns(root);

            var response = A.Fake<IFirebaseHttpResponseMessage>();

            var responses = new[]
            {
                "event: auth_revoked",
                "data:",
            };

            using (Stream stream = BuildResponseStream(responses))
            {
                A.CallTo(() => response.ReadAsStreamAsync()).Returns(stream);

                A.CallTo(() => client.SendAsync(
                    A<HttpRequestMessage>.That.Matches(
                        req => req.MatchStreaming(HttpMethod.Get, expectedUri, "text/event-stream")),
                    A<HttpCompletionOption>.Ignored,
                    A<CancellationToken>.Ignored)).Returns(response);

                var addedCallback = A.Fake<ValueAddedEventHandler>();
                var changedCallback = A.Fake<ValueChangedEventHandler>();
                var removedCallback = A.Fake<ValueRemovedEventHandler>();
                var closedCallback = A.Fake<StreamingResponseClosedHandler>();

                FirebaseRequest firebaseFirebaseRequest = new FirebaseRequest(client, null);
                var result = firebaseFirebaseRequest.GetStreaming(
                    path: childPath,
                    cancellationToken: CancellationToken.None).Result;

                result.Added += addedCallback;
                result.Changed += changedCallback;
                result.Removed += removedCallback;
                result.Closed += closedCallback;

                ManualResetEvent revoked = new ManualResetEvent(false);
                result.Revoked += (sender, e) => { revoked.Set(); };

                result.Listen();

                Assert.IsTrue(revoked.WaitOne(TimeSpan.FromSeconds(5)));

                A.CallTo(() => addedCallback.Invoke(A<object>._, A<ValueAddedEventArgs>._)).MustNotHaveHappened();
                A.CallTo(() => changedCallback.Invoke(A<object>._, A<ValueChangedEventArgs>._)).MustNotHaveHappened();
                A.CallTo(() => removedCallback.Invoke(A<object>._, A<ValueRemovedEventArgs>._)).MustNotHaveHappened();
                A.CallTo(() => closedCallback.Invoke(A<object>._, A<StreamingResponseClosedEventArgs>._)).MustHaveHappened();
            }
        }

        [TestMethod]
        public void CancelFireCanceled()
        {
            Uri root = new Uri("http://example.com/root");
            string childPath = "/item/path";
            Uri expectedUri = new Uri("http://example.com/root/item/path.json");

            var client = A.Fake<IFirebaseHttpClient>();
            A.CallTo(() => client.BaseAddress).Returns(root);

            var response = A.Fake<IFirebaseHttpResponseMessage>();

            var responses = new[]
            {
                "event: cancel",
                "data:",
            };

            using (Stream stream = BuildResponseStream(responses))
            {
                A.CallTo(() => response.ReadAsStreamAsync()).Returns(stream);

                A.CallTo(() => client.SendAsync(
                    A<HttpRequestMessage>.That.Matches(
                        req => req.MatchStreaming(HttpMethod.Get, expectedUri, "text/event-stream")),
                    A<HttpCompletionOption>.Ignored,
                    A<CancellationToken>.Ignored)).Returns(response);

                var addedCallback = A.Fake<ValueAddedEventHandler>();
                var changedCallback = A.Fake<ValueChangedEventHandler>();
                var removedCallback = A.Fake<ValueRemovedEventHandler>();
                var revokedCallback = A.Fake<AuthenticationRevokedHandler>();

                FirebaseRequest firebaseFirebaseRequest = new FirebaseRequest(client, null);
                var result = firebaseFirebaseRequest.GetStreaming(
                    path: childPath,
                    cancellationToken: CancellationToken.None).Result;

                result.Added += addedCallback;
                result.Changed += changedCallback;
                result.Removed += removedCallback;
                result.Revoked += revokedCallback;

                bool canceled = false;
                result.Canceled += (sender, e) =>
                {
                    canceled = true;
                };

                ManualResetEvent closed = new ManualResetEvent(false);
                result.Closed += (sender, e) => { closed.Set(); };

                result.Listen();

                Assert.IsTrue(closed.WaitOne(TimeSpan.FromSeconds(5)));
                Assert.IsTrue(canceled, "canceled event not fired");

                A.CallTo(() => addedCallback.Invoke(A<object>._, A<ValueAddedEventArgs>._)).MustNotHaveHappened();
                A.CallTo(() => changedCallback.Invoke(A<object>._, A<ValueChangedEventArgs>._)).MustNotHaveHappened();
                A.CallTo(() => removedCallback.Invoke(A<object>._, A<ValueRemovedEventArgs>._)).MustNotHaveHappened();
                A.CallTo(() => revokedCallback.Invoke(A<object>._, A<AuthenticationRevokedEventArgs>._)).MustNotHaveHappened();
            }
        }


        [TestMethod]
        public void DeleteFiresRemoved()
        {
            // {"path":"/10:17","data":null}

            Uri root = new Uri("http://example.com/root");
            string childPath = "/item/path";
            Uri expectedUri = new Uri("http://example.com/root/item/path.json");

            var client = A.Fake<IFirebaseHttpClient>();
            A.CallTo(() => client.BaseAddress).Returns(root);

            var response = A.Fake<IFirebaseHttpResponseMessage>();

            var responses = new[]
            {
                "event: put",
                "data: {\"path\":\"/10:17\",\"data\":null}",
            };

            using (Stream stream = BuildResponseStream(responses))
            {
                A.CallTo(() => response.ReadAsStreamAsync()).Returns(stream);

                A.CallTo(() => client.SendAsync(
                    A<HttpRequestMessage>.That.Matches(
                        req => req.MatchStreaming(HttpMethod.Get, expectedUri, "text/event-stream")),
                    A<HttpCompletionOption>.Ignored,
                    A<CancellationToken>.Ignored)).Returns(response);

                var addedCallback = A.Fake<ValueAddedEventHandler>();
                var changedCallback = A.Fake<ValueChangedEventHandler>();
                var revokedCallback = A.Fake<AuthenticationRevokedHandler>();

                FirebaseRequest firebaseFirebaseRequest = new FirebaseRequest(client, null);
                var result = firebaseFirebaseRequest.GetStreaming(
                    path: childPath,
                    cancellationToken: CancellationToken.None).Result;

                result.Added += addedCallback;
                result.Changed += changedCallback;
                result.Revoked += revokedCallback;

                ManualResetEvent closed = new ManualResetEvent(false);
                result.Closed += (sender, e) => { closed.Set(); };

                bool removedCalled = false;

                result.Removed += (sender, e) =>
                {
                    Assert.AreEqual("/10:17", e.Path);
                    removedCalled = true;
                };

                result.Listen();

                Assert.IsTrue(closed.WaitOne(TimeSpan.FromSeconds(5)));
                Assert.IsTrue(removedCalled, "removed event not fired");

                A.CallTo(() => addedCallback.Invoke(A<object>._, A<ValueAddedEventArgs>._)).MustNotHaveHappened();
                A.CallTo(() => changedCallback.Invoke(A<object>._, A<ValueChangedEventArgs>._)).MustNotHaveHappened();
                A.CallTo(() => revokedCallback.Invoke(A<object>._, A<AuthenticationRevokedEventArgs>._)).MustNotHaveHappened();
            }
        }


        [TestMethod]
        public void IdleTimeoutFires()
        {
            Uri root = new Uri("http://example.com/root");
            string childPath = "/item/path";
            Uri expectedUri = new Uri("http://example.com/root/item/path.json");

            var client = A.Fake<IFirebaseHttpClient>();
            A.CallTo(() => client.BaseAddress).Returns(root);

            var response = A.Fake<IFirebaseHttpResponseMessage>();

            A.CallTo(() => response.ReadAsStreamAsync()).ReturnsLazily((stream) =>
            {
                // we'll pretend that the streaming response threw
                // a timeout exception (we test that this happens later)
                throw new TimeoutException();
            });

            A.CallTo(() => client.SendAsync(
                A<HttpRequestMessage>.That.Matches(
                    req => req.MatchStreaming(HttpMethod.Get, expectedUri, "text/event-stream")),
                A<HttpCompletionOption>.Ignored,
                A<CancellationToken>.Ignored)).Returns(response);

            var addedCallback = A.Fake<ValueAddedEventHandler>();
            var changedCallback = A.Fake<ValueChangedEventHandler>();
            var revokedCallback = A.Fake<AuthenticationRevokedHandler>();
            var removedCallback = A.Fake<ValueRemovedEventHandler>();

            FirebaseRequest firebaseFirebaseRequest = new FirebaseRequest(client, null);
            var result = firebaseFirebaseRequest.GetStreaming(
                path: childPath,
                cancellationToken: CancellationToken.None).Result;

            result.Added += addedCallback;
            result.Changed += changedCallback;
            result.Revoked += revokedCallback;
            result.Removed += removedCallback;

            ManualResetEvent closed = new ManualResetEvent(false);
            result.Closed += (sender, e) => { closed.Set(); };

            bool timeoutCalled = false;

            result.Timeout += (sender, e) =>
            {
                timeoutCalled = true;
            };

            result.Listen();

            Assert.IsTrue(closed.WaitOne(TimeSpan.FromSeconds(5)));
            Assert.IsTrue(timeoutCalled, "timeout event not fired");

            A.CallTo(() => addedCallback.Invoke(A<object>._, A<ValueAddedEventArgs>._)).MustNotHaveHappened();
            A.CallTo(() => changedCallback.Invoke(A<object>._, A<ValueChangedEventArgs>._)).MustNotHaveHappened();
            A.CallTo(() => revokedCallback.Invoke(A<object>._, A<AuthenticationRevokedEventArgs>._)).MustNotHaveHappened();
            A.CallTo(() => removedCallback.Invoke(A<object>._, A<ValueRemovedEventArgs>._)).MustNotHaveHappened();
        }

        [TestMethod]
        public void ErrorFires()
        {
            Uri root = new Uri("http://example.com/root");
            string childPath = "/item/path";
            Uri expectedUri = new Uri("http://example.com/root/item/path.json");

            var client = A.Fake<IFirebaseHttpClient>();
            A.CallTo(() => client.BaseAddress).Returns(root);

            var response = A.Fake<IFirebaseHttpResponseMessage>();

            string expectedMessage = Guid.NewGuid().ToString();

            A.CallTo(() => response.ReadAsStreamAsync()).ReturnsLazily((stream) =>
            {
                // we'll pretend that the streaming response threw
                // a timeout exception (we test that this happens later)
                throw new Exception(expectedMessage);
            });

            A.CallTo(() => client.SendAsync(
                A<HttpRequestMessage>.That.Matches(
                    req => req.MatchStreaming(HttpMethod.Get, expectedUri, "text/event-stream")),
                A<HttpCompletionOption>.Ignored,
                A<CancellationToken>.Ignored)).Returns(response);

            var addedCallback = A.Fake<ValueAddedEventHandler>();
            var changedCallback = A.Fake<ValueChangedEventHandler>();
            var revokedCallback = A.Fake<AuthenticationRevokedHandler>();
            var removedCallback = A.Fake<ValueRemovedEventHandler>();
            var timeoutCallack = A.Fake<StreamingResponseIdleTimeoutHandler>();

            FirebaseRequest firebaseFirebaseRequest = new FirebaseRequest(client, null);
            var result = firebaseFirebaseRequest.GetStreaming(
                path: childPath,
                cancellationToken: CancellationToken.None).Result;

            result.Added += addedCallback;
            result.Changed += changedCallback;
            result.Revoked += revokedCallback;
            result.Removed += removedCallback;
            result.Timeout += timeoutCallack;

            ManualResetEvent closed = new ManualResetEvent(false);
            result.Closed += (sender, e) => { closed.Set(); };

            string errorMessage = null;

            result.Error += (sender, e) =>
            {
                errorMessage = e.Error.Message;
            };

            result.Listen();

            Assert.IsTrue(closed.WaitOne(TimeSpan.FromSeconds(5)));
            Assert.AreEqual(expectedMessage, errorMessage);

            A.CallTo(() => addedCallback.Invoke(A<object>._, A<ValueAddedEventArgs>._)).MustNotHaveHappened();
            A.CallTo(() => changedCallback.Invoke(A<object>._, A<ValueChangedEventArgs>._)).MustNotHaveHappened();
            A.CallTo(() => revokedCallback.Invoke(A<object>._, A<AuthenticationRevokedEventArgs>._)).MustNotHaveHappened();
            A.CallTo(() => removedCallback.Invoke(A<object>._, A<ValueRemovedEventArgs>._)).MustNotHaveHappened();
            A.CallTo(() => timeoutCallack.Invoke(A<object>._, A<StreamingResponseIdleTimeoutEventArgs>._)).MustNotHaveHappened();
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