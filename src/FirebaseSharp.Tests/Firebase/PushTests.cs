using System;
using System.Threading;
using FirebaseSharp.Portable;
using FirebaseSharp.Portable.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Tests.Firebase
{
    [TestClass]
    public class PushTests
    {
        [TestMethod]
        [Ignore]
        public void PushPushes()
        {
            FirebaseApp app = new FirebaseApp(new Uri("https://todo.example.com/"));
            IFirebase list = app.Child("message_list");

            IFirebase post1Ref = list.Push("{ \"name\": \"Post 1\" }", error => { });

            ManualResetEvent mre = new ManualResetEvent(false);
            post1Ref.Once("value", (snap, prev, ctx) =>
            {
                JToken actual = JToken.Parse(snap.Value());
                Assert.IsNotNull(actual["name"]);
                JValue name = (JValue)actual["name"];
                Assert.AreEqual("Post 1", name.Value);
                mre.Set();
            });

            Assert.IsTrue(mre.WaitOne(TimeSpan.FromSeconds(5)), "Callback never fired");
        }
    }
}
