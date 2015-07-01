using System;
using System.Net.Http;
using System.Threading;
using FirebaseSharp.Portable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FakeItEasy;
using FirebaseSharp.Portable.Request;
using FirebaseSharp.Portable.Response;

namespace FirebaseSharp.Tests
{
    [TestClass]
    public class FirebaseSendTests
    {
        [TestMethod]
        public void Get()
        {
            Uri root = new Uri("http://example.com/root");
            string childPath = "/item/path";
            Uri expectedUri = new Uri("http://example.com/root/item/path.json");

            string storedValue = Guid.NewGuid().ToString();

            var client = A.Fake<IFirebaseHttpClient>();
            A.CallTo(() => client.BaseAddress).Returns(root);

            var response = A.Fake<IFirebaseHttpResponseMessage>();
            A.CallTo(() => response.ReadAsStringAsync(A<CancellationToken>.Ignored)).Returns(storedValue);

            var call = A.CallTo(() => client.SendAsync(
                A<HttpRequestMessage>.That.Matches(req => req.Matches(HttpMethod.Get, expectedUri)),
                A<HttpCompletionOption>.Ignored,
                A<CancellationToken>.Ignored));

            call.Returns(response);

            using (FirebaseRequest firebaseFirebaseRequest = new FirebaseRequest(client, null))
            {
                string result = firebaseFirebaseRequest.GetSingle(childPath, CancellationToken.None).Result;
                Assert.AreEqual(storedValue, result);
            }

            call.MustHaveHappened();
            A.CallTo(() => response.EnsureSuccessStatusCode()).MustHaveHappened();
            A.CallTo(() => client.Dispose()).MustHaveHappened();
        }

        [TestMethod]
        public void Delete()
        {
            Uri root = new Uri("http://example.com/root");
            string childPath = "/item/path";
            Uri expectedUri = new Uri("http://example.com/root/item/path.json");

            var client = A.Fake<IFirebaseHttpClient>();
            A.CallTo(() => client.BaseAddress).Returns(root);

            var response = A.Fake<IFirebaseHttpResponseMessage>();

            A.CallTo(() => client.SendAsync(
                A<HttpRequestMessage>.That.Matches(req => req.Matches(HttpMethod.Delete, expectedUri)),
                A<HttpCompletionOption>.Ignored,
                A<CancellationToken>.Ignored)).Returns(response);

            FirebaseRequest firebaseFirebaseRequest = new FirebaseRequest(client, null);
            firebaseFirebaseRequest.Delete(childPath, CancellationToken.None).Wait();

            A.CallTo(() => response.EnsureSuccessStatusCode()).MustHaveHappened();
            A.CallTo(() => response.ReadAsStringAsync(A<CancellationToken>.Ignored)).MustNotHaveHappened();
            A.CallTo(() => response.ReadAsStreamAsync(A<CancellationToken>.Ignored)).MustNotHaveHappened();
        }

        [TestMethod]
        public void Post()
        {
            Uri root = new Uri("http://example.com/root");
            string childPath = "/item/path";
            Uri expectedUri = new Uri("http://example.com/root/item/path.json");

            string queryPayload = Guid.NewGuid().ToString();
            string responsePayload = Guid.NewGuid().ToString();

            var client = A.Fake<IFirebaseHttpClient>();
            A.CallTo(() => client.BaseAddress).Returns(root);

            var response = A.Fake<IFirebaseHttpResponseMessage>();

            A.CallTo(() => response.ReadAsStringAsync(A<CancellationToken>.Ignored)).Returns(responsePayload);

            A.CallTo(() => client.SendAsync(
                A<HttpRequestMessage>.That.Matches(req => req.Matches(HttpMethod.Post, expectedUri, queryPayload)),
                A<HttpCompletionOption>.Ignored,
                A<CancellationToken>.Ignored)).Returns(response);

            FirebaseRequest firebaseFirebaseRequest = new FirebaseRequest(client, null);
            var result = firebaseFirebaseRequest.Post(childPath, queryPayload, CancellationToken.None).Result;

            Assert.AreEqual(responsePayload, result);

            A.CallTo(() => response.EnsureSuccessStatusCode()).MustHaveHappened();
            A.CallTo(() => response.ReadAsStringAsync(A<CancellationToken>.Ignored)).MustHaveHappened();
            A.CallTo(() => response.ReadAsStreamAsync(A<CancellationToken>.Ignored)).MustNotHaveHappened();
        }

        [TestMethod]
        public void Put()
        {
            Uri root = new Uri("http://example.com/root");
            string childPath = "/item/path";
            Uri expectedUri = new Uri("http://example.com/root/item/path.json");

            string queryPayload = Guid.NewGuid().ToString();
            string responsePayload = Guid.NewGuid().ToString();

            var client = A.Fake<IFirebaseHttpClient>();
            A.CallTo(() => client.BaseAddress).Returns(root);

            var response = A.Fake<IFirebaseHttpResponseMessage>();

            A.CallTo(() => response.ReadAsStringAsync(A<CancellationToken>.Ignored)).Returns(responsePayload);

            A.CallTo(() => client.SendAsync(
                A<HttpRequestMessage>.That.Matches(req => req.Matches(HttpMethod.Put, expectedUri, queryPayload)),
                A<HttpCompletionOption>.Ignored,
                A<CancellationToken>.Ignored)).Returns(response);

            FirebaseRequest firebaseFirebaseRequest = new FirebaseRequest(client, null);
            var result = firebaseFirebaseRequest.Put(childPath, queryPayload, CancellationToken.None).Result;

            Assert.AreEqual(responsePayload, result);

            A.CallTo(() => response.EnsureSuccessStatusCode()).MustHaveHappened();
            A.CallTo(() => response.ReadAsStringAsync(A<CancellationToken>.Ignored)).MustHaveHappened();
            A.CallTo(() => response.ReadAsStreamAsync(A<CancellationToken>.Ignored)).MustNotHaveHappened();
        }

        [TestMethod]
        public void Patch()
        {
            Uri root = new Uri("http://example.com/root");
            string childPath = "/item/path";
            Uri expectedUri = new Uri("http://example.com/root/item/path.json");

            string queryPayload = Guid.NewGuid().ToString();
            string responsePayload = Guid.NewGuid().ToString();

            var client = A.Fake<IFirebaseHttpClient>();
            A.CallTo(() => client.BaseAddress).Returns(root);

            var response = A.Fake<IFirebaseHttpResponseMessage>();

            A.CallTo(() => response.ReadAsStringAsync(A<CancellationToken>.Ignored)).Returns(responsePayload);

            A.CallTo(() => client.SendAsync(
                A<HttpRequestMessage>.That.Matches(req => req.Matches(new HttpMethod("PATCH"), expectedUri, queryPayload)),
                A<HttpCompletionOption>.Ignored,
                A<CancellationToken>.Ignored)).Returns(response);

            FirebaseRequest firebaseFirebaseRequest = new FirebaseRequest(client, null);
            var result = firebaseFirebaseRequest.Patch(childPath, queryPayload, CancellationToken.None).Result;

            Assert.AreEqual(responsePayload, result);

            A.CallTo(() => response.EnsureSuccessStatusCode()).MustHaveHappened();
            A.CallTo(() => response.ReadAsStringAsync(A<CancellationToken>.Ignored)).MustHaveHappened();
            A.CallTo(() => response.ReadAsStreamAsync(A<CancellationToken>.Ignored)).MustNotHaveHappened();
        }

    }
}
