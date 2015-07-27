using System;
using System.Linq;
using System.Threading;
using FirebaseSharp.Portable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirebaseSharp.Tests.Filter
{
    [TestClass]
    public class StartAt
    {
        [TestMethod]
        public void StartAtNumeric()
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

            RunTest(json, "test1", 1, new[] {"keyAAA", "keyBBB", "keyCCC"});
            RunTest(json, "test1", 2, new[] {"keyBBB", "keyCCC"});
            RunTest(json, "test1", 3, new[] {"keyCCC"});
            RunTest(json, "test1", 4, new string[0]);

            RunTest(json, "test2", 1, new[] {"keyAAA", "keyBBB", "keyCCC"});
            RunTest(json, "test2", 2, new string[0]);
            RunTest(json, "test2", 3, new string[0]);
            RunTest(json, "test2", 4, new string[0]);

            RunTest(json, "test3", 1, new[] {"keyAAA", "keyBBB", "keyCCC"});
            RunTest(json, "test3", 2, new[] {"keyAAA", "keyBBB", "keyCCC"});
            RunTest(json, "test3", 3, new[] {"keyCCC"});
            RunTest(json, "test3", 4, new string[0]);
        }

        [TestMethod]
        public void StartAtString()
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

            RunTest(json, "test1", "a", new[] {"keyAAA", "keyBBB", "keyCCC"});
            RunTest(json, "test1", "b", new[] {"keyBBB", "keyCCC"});
            RunTest(json, "test1", "c", new[] {"keyCCC"});
            RunTest(json, "test1", "d", new string[0]);

            RunTest(json, "test2", "a", new[] {"keyAAA", "keyBBB", "keyCCC"});
            RunTest(json, "test2", "b", new string[0]);
            RunTest(json, "test2", "c", new string[0]);
            RunTest(json, "test2", "d", new string[0]);

            RunTest(json, "test3", "a", new[] {"keyAAA", "keyBBB", "keyCCC"});
            RunTest(json, "test3", "b", new[] {"keyAAA", "keyBBB", "keyCCC"});
            RunTest(json, "test3", "c", new[] {"keyCCC"});
            RunTest(json, "test3", "d", new string[0]);
        }

        [TestMethod]
        public void StartAtKey()
        {
            string json = @"
{
    'keyAAA': {
        'test1': 1
    },
    'keyBBB': {
        'test1': 2
    },
    'keyCCC': {
        'test1': 3
    },
}
";

            RunKeyTest(json, "test1", "key", new[] { "keyAAA", "keyBBB", "keyCCC" });
            RunKeyTest(json, "test1", "keyAAA", new[] { "keyAAA", "keyBBB", "keyCCC" });
            RunKeyTest(json, "test1", "keyBBB", new[] { "keyBBB", "keyCCC" });
            RunKeyTest(json, "test1", "keyCCC", new[] { "keyCCC" });
            RunKeyTest(json, "test1", "keyDDD", new string[0]);
        }

        private void RunKeyTest(string json, string testName, string startAt, string[] expectedOrder)
        {
            using (FirebaseApp app = AppFactory.FromJson(json))
            {
                ManualResetEvent fired = new ManualResetEvent(false);
                var query = app.Child("/")
                    .OrderByChild(testName)
                    .StartAtKey(startAt)
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

                Assert.IsTrue(fired.WaitOne(TimeSpan.FromSeconds(555)), "callback did not fire during " + testName);
            }
        }

        private void RunTest(string json, string testName, string startAt, string[] expectedOrder)
        {
            using (FirebaseApp app = AppFactory.FromJson(json))
            {
                ManualResetEvent fired = new ManualResetEvent(false);
                var query = app.Child("/")
                    .OrderByChild(testName)
                    .StartAt(startAt)
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

        private void RunTest(string json, string testName, long startAt, string[] expectedOrder)
        {
            using (FirebaseApp app = AppFactory.FromJson(json))
            {
                ManualResetEvent fired = new ManualResetEvent(false);
                var query = app.Child("/")
                    .OrderByChild(testName)
                    .StartAt(startAt)
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