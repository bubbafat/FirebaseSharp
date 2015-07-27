using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FirebaseSharp.Portable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirebaseSharp.Tests.Firebase
{
    [TestClass]
    public class PushTests
    {
        [TestMethod]
        public void PushPushes()
        {
            using (FirebaseApp app = AppFactory.Empty())
            {
                ManualResetEvent done = new ManualResetEvent(false);

                var list = app.Child("message_list");
                List<string> ids = new List<string>();

                ids.Add(list.Push("{'name':'Post 1'}").Key);
                ids.Add(list.Push("{'name':'Post 2'}").Key);
                ids.Add(list.Push("{'name':'Post 3'}").Key);

                list.Once("value", (snap, child, context) =>
                {
                    var children = snap.Children.ToList();
                    Assert.AreEqual(ids[0], children[0].Key);
                    Assert.AreEqual(ids[1], children[1].Key);
                    Assert.AreEqual(ids[2], children[2].Key);

                    Assert.AreEqual("Post 1", children[0]["name"].Value());
                    Assert.AreEqual("Post 2", children[1]["name"].Value());
                    Assert.AreEqual("Post 3", children[2]["name"].Value());

                    done.Set();
                });

                Assert.IsTrue(done.WaitOne(TimeSpan.FromSeconds(15)), "callback never fired");
            }
        }

        [TestMethod]
        public void PushEmptyCreatesChild()
        {
            using (FirebaseApp app = AppFactory.Empty())
            {
                ManualResetEvent done = new ManualResetEvent(false);

                var list = app.Child("message_list");
                List<string> ids = new List<string>();

                for (int i = 1; i < 4; i++)
                {
                    var pushRef = list.Push();
                    ids.Add(pushRef.Key);
                    pushRef.Set(string.Format("{{'name':'Post {0}'}}", i));
                }

                list.Once("value", (snap, child, context) =>
                {
                    var children = snap.Children.ToList();
                    Assert.AreEqual(ids[0], children[0].Key);
                    Assert.AreEqual(ids[1], children[1].Key);
                    Assert.AreEqual(ids[2], children[2].Key);

                    Assert.AreEqual("Post 1", children[0]["name"].Value());
                    Assert.AreEqual("Post 2", children[1]["name"].Value());
                    Assert.AreEqual("Post 3", children[2]["name"].Value());

                    done.Set();
                });

                Assert.IsTrue(done.WaitOne(TimeSpan.FromSeconds(15)), "callback never fired");
            }
        }
    }
}