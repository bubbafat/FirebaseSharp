using System;
using System.Linq;
using System.Threading;
using FirebaseSharp.Portable;
using FirebaseSharp.Portable.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirebaseSharp.Tests.Filter
{
    [TestClass]
    public class OrderByChild
    {
        [TestMethod]
        public void NullsTests()
        {
            string json =
                "{\"keyXXX\":{\"test1\":null,\"test2\":{},\"test3\":null},\"keyAAA\":{\"test1\":{},\"test2\":null,\"test3\":null}}";

            RunTest(json, "test1", new[] { "keyXXX", "keyAAA" });
            RunTest(json, "test2", new[] { "keyAAA", "keyXXX" });
            RunTest(json, "test3", new[] { "keyAAA", "keyXXX" });
        }

        private void RunTest(string json, string testName, string[] expectedOrder)
        {
            using (FirebaseApp app = AppFactory.FromJson(json))
            {
                ManualResetEvent fired = new ManualResetEvent(false);
                IFirebase limited = app.Child("/")
                    .OrderByChild(testName)
                    .On("value", (snap, previous, context) =>
                    {
                        Assert.IsNotNull(snap.Children, testName);
                        var children = snap.Children.ToList();

                        Assert.AreEqual(expectedOrder.Length, children.Count, testName);

                        for (int i = 0; i < expectedOrder.Length; i++)
                        {
                            Assert.AreEqual(expectedOrder[i], children[i].Key, testName);
                        }

                        fired.Set();
                    });

                Assert.IsTrue(fired.WaitOne(TimeSpan.FromSeconds(5)), "callback did not fire during " + testName);
            }
        }

    }
}
