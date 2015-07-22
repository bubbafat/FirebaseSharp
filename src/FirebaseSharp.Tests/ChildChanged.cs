using System;
using System.Collections.Generic;
using System.Threading;
using FirebaseSharp.Portable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

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
            using (_app) { }
        }

        [TestMethod]
        public void Basic()
        {
            List<Tuple<string, string, string>> expected = new List<Tuple<string, string, string>>
            {
                new Tuple<string, string, string>("put", @"{'foo': 'bar'}", @"{'foo': 'bar'}"),
                new Tuple<string, string, string>("patch", @"{'foo': 'baz'}", @"{'foo': 'baz'}"),
                new Tuple<string, string, string>("put", @"{'foo': 'pipo'}", @"{'foo': 'pipo'}"),
            };

            ManualResetEvent done = new ManualResetEvent(false);

            int[] counter = new []{ 0 };
            var loc = _app.Child("/").On("child_changed", (snap, child, context) =>
            {
                JToken expect = JToken.Parse(expected[counter[0]].Item3);
                string actualStr = snap.Value();
                JToken actual = JToken.Parse(actualStr);
                Assert.IsTrue(JToken.DeepEquals(expect, actual));
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
