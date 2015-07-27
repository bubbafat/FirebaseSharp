using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using FakeItEasy;
using FirebaseSharp.Portable;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Tests.JsonCache
{
    [TestClass]
    public class IntegrityTests
    {
        private readonly string _weather;

        public IntegrityTests()
        {
            _weather = LoadData("weather.json");
        }

        private string LoadData(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var name =
                assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(string.Format(".{0}", fileName)));

            using (Stream stream = assembly.GetManifestResourceStream(name))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        [TestMethod]
        public void SetRootMatchesInput()
        {
            var expected = JToken.Parse(_weather);
            var client = A.Fake<IFirebaseNetworkConnection>();

            A.CallTo(() => client.Send(A<FirebaseMessage>._)).Invokes((FirebaseMessage message) =>
            {
                Assert.AreEqual(WriteBehavior.Replace, message.Behavior);
                Assert.AreEqual(new FirebasePath(), message.Path);
                Assert.IsTrue(JToken.DeepEquals(expected, JToken.Parse(message.Value)),
                    "The contents being written did not match the provided contents");

                message.Callback(null);
            });

            var jc = new SyncDatabase(null, client);

            using (var mre = new ManualResetEvent(false))
            {
                jc.Set(new FirebasePath(), _weather, (error) => { mre.Set(); });

                Assert.IsTrue(mre.WaitOne(TimeSpan.FromSeconds(5)),
                    "Time out waiting for cache callback");
            }

            Assert.IsTrue(JToken.DeepEquals(expected, JToken.Parse(jc.Dump())),
                "The cache contents did not match the expected structure");
        }

        [TestMethod]
        public void UpdateRootMatchesInput()
        {
            var expected = JToken.Parse(_weather);
            var client = A.Fake<IFirebaseNetworkConnection>();

            A.CallTo(() => client.Send(A<FirebaseMessage>._)).Invokes((FirebaseMessage message) =>
            {
                Assert.AreEqual(WriteBehavior.Merge, message.Behavior);
                Assert.AreEqual(new FirebasePath(), message.Path);
                Assert.IsTrue(JToken.DeepEquals(expected, JToken.Parse(message.Value)),
                    "The contents being written did not match the provided contents");

                message.Callback(null);
            });

            var jc = new SyncDatabase(null, client);

            using (var mre = new ManualResetEvent(false))
            {
                jc.Update(new FirebasePath(), _weather, (error) => { mre.Set(); });

                Assert.IsTrue(mre.WaitOne(TimeSpan.FromSeconds(5)),
                    "Time out waiting for cache callback");
            }

            Assert.IsTrue(JToken.DeepEquals(expected, JToken.Parse(jc.Dump())),
                "The cache contents did not match the expected structure");
        }


        [TestMethod]
        public void MultiUpdateMatchesOutput()
        {
            var data = new List<Tuple<Tuple<string, string>, string>>()
            {
                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/me",
                        "{\"name\": \"Robert\"}"),
                    "{\"people\": { \"me\": {\"name\": \"Robert\"}}}"),
                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/me/name",
                        "Bob"),
                    "{\"people\": { \"me\": {\"name\": \"Bob\"}}}"),
                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/you",
                        "{\"name\": \"Susan\"}"),
                    "{\"people\": { \"me\": {\"name\": \"Bob\"}, \"you\": {\"name\": \"Susan\"}}}"),
                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/me/age",
                        "38"),
                    "{\"people\": { \"me\": {\"name\": \"Bob\", \"age\": \"38\"}, \"you\": {\"name\": \"Susan\"}}}"),
                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/me",
                        "{\"name\": \"Bobby\"}"),
                    "{\"people\": { \"me\": {\"name\": \"Bobby\", \"age\": \"38\"}, \"you\": {\"name\": \"Susan\"}}}"),
                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/",
                        "{ \"me\": {\"name\": \"Bobert\"}}"),
                    "{\"people\": { \"me\": {\"name\": \"Bobert\" }, \"you\": {\"name\": \"Susan\"}}}"),

                // nulls delete
                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/me",
                        "{\"name\": null}"),
                    "{\"people\": { \"me\": { }, \"you\": {\"name\": \"Susan\"}}}"),
            };

            var client = A.Fake<IFirebaseNetworkConnection>();
            A.CallTo(() => client.Send(A<FirebaseMessage>._))
                .Invokes((FirebaseMessage message) => { message.Callback(null); });

            var jc = new SyncDatabase(null, client);
            ManualResetEvent called = new ManualResetEvent(false);

            foreach (var item in data)
            {
                called.Reset();
                string path = item.Item1.Item1;
                string value = item.Item1.Item2;
                var expected = JToken.Parse(item.Item2);
                jc.Update(new FirebasePath(path), value, error =>
                {
                    JToken actual = JToken.Parse(jc.Dump());
                    Assert.IsTrue(JToken.DeepEquals(expected, actual),
                        "The cache state did not match the expected cache state");
                    called.Set();
                });

                Assert.IsTrue(called.WaitOne(TimeSpan.FromSeconds(2)), "The callback never fired");
            }
        }

        [TestMethod]
        public void MultiSetMatchesOutput()
        {
            var data = new List<Tuple<Tuple<string, string>, string>>()
            {
                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/me",
                        "{\"name\": \"Robert\"}"),
                    "{\"people\": { \"me\": {\"name\": \"Robert\"}}}"),
                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/me/name",
                        "Bob"),
                    "{\"people\": { \"me\": {\"name\": \"Bob\"}}}"),
                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/you",
                        "{\"name\": \"Susan\"}"),
                    "{\"people\": { \"me\": {\"name\": \"Bob\"}, \"you\": {\"name\": \"Susan\"}}}"),
                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/me/age",
                        "38"),
                    "{\"people\": { \"me\": {\"name\": \"Bob\", \"age\": \"38\"}, \"you\": {\"name\": \"Susan\"}}}"),
                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/me",
                        "{\"name\": \"Bobby\"}"),
                    "{\"people\": { \"me\": {\"name\": \"Bobby\" }, \"you\": {\"name\": \"Susan\"}}}"),
                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/",
                        "{ \"me\": {\"name\": \"Bobert\"}}"),
                    "{\"people\": { \"me\": {\"name\": \"Bobert\" }}}"),
                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/me/name",
                        null),
                    "{\"people\": { \"me\": {}}}"),
            };

            var client = A.Fake<IFirebaseNetworkConnection>();
            A.CallTo(() => client.Send(A<FirebaseMessage>._))
                .Invokes((FirebaseMessage message) => { message.Callback(null); });


            var jc = new SyncDatabase(null, client);
            ManualResetEvent called = new ManualResetEvent(false);

            foreach (var item in data)
            {
                called.Reset();
                string path = item.Item1.Item1;
                string value = item.Item1.Item2;
                var expected = JToken.Parse(item.Item2);
                jc.Set(new FirebasePath(path), value, error =>
                {
                    JToken actual = JToken.Parse(jc.Dump());
                    Assert.IsTrue(JToken.DeepEquals(expected, actual),
                        "The cache state did not match the expected cache state");
                    called.Set();
                });

                Assert.IsTrue(called.WaitOne(TimeSpan.FromSeconds(2)), "The callback never fired");
            }
        }
    }
}