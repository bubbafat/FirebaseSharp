using System;
using System.Collections.Generic;
using System.Threading;
using FirebaseSharp.Portable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirebaseSharp.Tests
{
    [TestClass]
    public class ChildChanged
    {
        private IFirebaseApp _app;

        [TestInitialize]
        public void TestInit()
        {
            _app = AppFactory.Empty();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            using (_app)
            {
            }
        }

        [TestMethod]
        public void Basic()
        {
            List<Tuple<string, string, string>> expected = new List<Tuple<string, string, string>>
            {
                new Tuple<string, string, string>("put", @"{'foo': 'bar'}", "bar"),
                new Tuple<string, string, string>("patch", @"{'foo': 'baz'}", "baz"),
                new Tuple<string, string, string>("put", @"{'foo': 'pipo'}", "pipo"),
            };

            ManualResetEvent done = new ManualResetEvent(false);

            int[] counter = new[] {0};
            var loc = _app.Child("/").On("child_changed", (snap, child, context) =>
            {
                Assert.AreEqual("foo", snap.Key);
                Assert.AreEqual(expected[counter[0]].Item3, snap.Value());
                if (++counter[0] == 3)
                {
                    done.Set();
                }
            });

            foreach (var test in expected)
            {
                switch (test.Item1)
                {
                    case "put":
                        loc.Ref().Set(test.Item2);
                        break;
                    case "patch":
                        loc.Ref().Update(test.Item2);
                        break;
                    default:
                        Assert.Fail("Unknown action: {0}", test.Item1);
                        break;
                }
            }

            Assert.IsTrue(done.WaitOne(TimeSpan.FromSeconds(5)), "Callback did not fire enough: " + counter.ToString());
        }
    }
}