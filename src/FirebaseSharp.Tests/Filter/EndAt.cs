using System;
using System.Linq;
using System.Threading;
using FirebaseSharp.Portable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirebaseSharp.Tests.Filter
{
    [TestClass]
    public class EndAt
    {
        [TestMethod]
        public void EndAt_Numeric()
        {
            string json = @"
{
    'keyAAA': {
        'test1': 1,
        'test2': 1,
        'test3': 2
    },
    'keyBBB': {
        'test1': 2,
        'test2': 1,
        'test3': 2
    },
    'keyCCC': {
        'test1': 3,
        'test2': 1,
        'test3': 3
    },
}
";

            RunTest(json, "test1", 4, new[] {"keyAAA", "keyBBB", "keyCCC"});
            RunTest(json, "test1", 3, new[] {"keyAAA", "keyBBB", "keyCCC"});
            RunTest(json, "test1", 2, new[] {"keyAAA", "keyBBB"});
            RunTest(json, "test1", 1, new[] {"keyAAA"});
            RunTest(json, "test1", 0, new string[0]);

            RunTest(json, "test2", 4, new[] {"keyAAA", "keyBBB", "keyCCC"});
            RunTest(json, "test2", 3, new[] {"keyAAA", "keyBBB", "keyCCC"});
            RunTest(json, "test2", 2, new[] {"keyAAA", "keyBBB", "keyCCC"});
            RunTest(json, "test2", 1, new[] {"keyAAA", "keyBBB", "keyCCC"});
            RunTest(json, "test1", -1, new string[0]);

            RunTest(json, "test3", 4, new[] {"keyAAA", "keyBBB", "keyCCC"});
            RunTest(json, "test3", 3, new[] {"keyAAA", "keyBBB", "keyCCC"});
            RunTest(json, "test3", 2, new[] {"keyAAA", "keyBBB"});
            RunTest(json, "test3", 1, new string[0]);
        }

        [TestMethod]
        public void EndAt_String()
        {
            string json = @"
{
    'keyAAA': {
        'test1': 'a',
        'test2': 'a',
        'test3': 'b'
    },
    'keyBBB': {
        'test1': 'b',
        'test2': 'a',
        'test3': 'b'
    },
    'keyCCC': {
        'test1': 'c',
        'test2': 'a',
        'test3': 'c'
    },
}
";

            RunTest(json, "test1", "d", new[] {"keyAAA", "keyBBB", "keyCCC"});
            RunTest(json, "test1", "c", new[] {"keyAAA", "keyBBB", "keyCCC"});
            RunTest(json, "test1", "b", new[] {"keyAAA", "keyBBB"});
            RunTest(json, "test1", "a", new[] {"keyAAA"});
            RunTest(json, "test1", null, new string[0]);

            RunTest(json, "test2", "d", new[] {"keyAAA", "keyBBB", "keyCCC"});
            RunTest(json, "test2", "c", new[] {"keyAAA", "keyBBB", "keyCCC"});
            RunTest(json, "test2", "b", new[] {"keyAAA", "keyBBB", "keyCCC"});
            RunTest(json, "test2", "a", new[] {"keyAAA", "keyBBB", "keyCCC"});
            RunTest(json, "test1", "$", new string[0]);

            RunTest(json, "test3", "d", new[] {"keyAAA", "keyBBB", "keyCCC"});
            RunTest(json, "test3", "c", new[] {"keyAAA", "keyBBB", "keyCCC"});
            RunTest(json, "test3", "b", new[] {"keyAAA", "keyBBB"});
            RunTest(json, "test3", "a", new string[0]);
        }

        [TestMethod]
        public void EndAt_Key()
        {
            string json = @"
{
    'keyAAA': {
        'test1': 'a',
        'test2': 'a',
        'test3': 'b'
    },
    'keyBBB': {
        'test1': 'b',
        'test2': 'a',
        'test3': 'b'
    },
    'keyCCC': {
        'test1': 'c',
        'test2': 'a',
        'test3': 'c'
    },
}
";

            RunKeyTest(json, "test1", "zzz", new[] { "keyAAA", "keyBBB", "keyCCC" });
            RunKeyTest(json, "test1", "keyCCC", new[] { "keyAAA", "keyBBB", "keyCCC" });
            RunKeyTest(json, "test1", "keyBBB", new[] { "keyAAA", "keyBBB" });
            RunKeyTest(json, "test1", "keyAAA", new[] { "keyAAA" });
            RunKeyTest(json, "test1", "a", new string[0]);
            RunKeyTest(json, "test1", null, new string[0]);
        }

        private void RunKeyTest(string json, string testName, string endAt, string[] expectedOrder)
        {
            using (FirebaseApp app = AppFactory.FromJson(json))
            {
                ManualResetEvent fired = new ManualResetEvent(false);
                var query = app.Child("/")
                    .OrderByChild(testName)
                    .EndAtKey(endAt)
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


        private void RunTest(string json, string testName, string endAt, string[] expectedOrder)
        {
            using (FirebaseApp app = AppFactory.FromJson(json))
            {
                ManualResetEvent fired = new ManualResetEvent(false);
                var query = app.Child("/")
                    .OrderByChild(testName)
                    .EndAt(endAt)
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

        private void RunTest(string json, string testName, long endAt, string[] expectedOrder)
        {
            using (FirebaseApp app = AppFactory.FromJson(json))
            {
                ManualResetEvent fired = new ManualResetEvent(false);
                var query = app.Child("/")
                    .OrderByChild(testName)
                    .EndAt(endAt)
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