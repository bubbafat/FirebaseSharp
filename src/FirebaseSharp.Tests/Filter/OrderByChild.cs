using System;
using System.Linq;
using System.Threading;
using FirebaseSharp.Portable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirebaseSharp.Tests.Filter
{
    [TestClass]
    public class OrderByChild
    {
        [TestMethod]
        public void NullsTests()
        {
            string json = @"
{
    'keyXXX': {
        'test1':null,
        'test2':{},
        'test3':null
    },
    'keyAAA': {
        'test1':{},
        'test2':null,
        'test3':null
    }
}";

            RunTest(json, "test1", new[] {"keyXXX", "keyAAA"});
            RunTest(json, "test2", new[] {"keyAAA", "keyXXX"});
            RunTest(json, "test3", new[] {"keyAAA", "keyXXX"});
        }

        [TestMethod]
        public void BooleanTests()
        {
            string json = @"
{
    'keyXXX': {
        'test1': false,
        'test2': false,
        'test3': false
    },
    'keyAAA': {
        'test1': true,
        'test2': false,
        'test3': 'aaa'
    },
    'keyBBB': {
        'test1': false,
        'test2': false,
        'test3': null
    },
}
";

            RunTest(json, "test1", new[] {"keyBBB", "keyXXX", "keyAAA"});
            RunTest(json, "test2", new[] {"keyAAA", "keyBBB", "keyXXX"});
            RunTest(json, "test3", new[] {"keyBBB", "keyXXX", "keyAAA"});
        }

        [TestMethod]
        public void NumericIntTests()
        {
            string json = @"
{
    'keyXXX': {
        'test1': 1,
        'test2': 3,
        'test3': 10
    },
    'keyAAA': {
        'test1': 2,
        'test2': 1,
        'test3': 10
    },
    'keyBBB': {
        'test1': 3,
        'test2': 2,
        'test3': 10
    },
}
";

            RunTest(json, "test1", new[] {"keyXXX", "keyAAA", "keyBBB"});
            RunTest(json, "test2", new[] {"keyAAA", "keyBBB", "keyXXX"});
            RunTest(json, "test3", new[] {"keyAAA", "keyBBB", "keyXXX"});
        }

        [TestMethod]
        public void NumericFloatTests()
        {
            string json = @"
{
    'keyXXX': {
        'test1': 1.0,
        'test2': 3.0,
        'test3': 10.0
    },
    'keyAAA': {
        'test1': 2.0,
        'test2': 1.0,
        'test3': 10.0
    },
    'keyBBB': {
        'test1': 3.0,
        'test2': 2.0,
        'test3': 10.0
    },
}
";

            RunTest(json, "test1", new[] {"keyXXX", "keyAAA", "keyBBB"});
            RunTest(json, "test2", new[] {"keyAAA", "keyBBB", "keyXXX"});
            RunTest(json, "test3", new[] {"keyAAA", "keyBBB", "keyXXX"});
        }

        [TestMethod]
        public void NumericMixedTests()
        {
            string json = @"
{
    'keyXXX': {
        'test1': 1,
        'test2': 3.0,
        'test3': 10
    },
    'keyAAA': {
        'test1': 2,
        'test2': 1,
        'test3': 10
    },
    'keyBBB': {
        'test1': 3.0,
        'test2': 2.0,
        'test3': 10.0
    },
}
";

            RunTest(json, "test1", new[] {"keyXXX", "keyAAA", "keyBBB"});
            RunTest(json, "test2", new[] {"keyAAA", "keyBBB", "keyXXX"});
            RunTest(json, "test3", new[] {"keyAAA", "keyBBB", "keyXXX"});
        }


        [TestMethod]
        public void StringTests()
        {
            string json = @"
{
    'keyXXX': {
        'test1': '1',
        'test2': 'aaa',
        'test3': 'xxx',
        'test4': 'aaa',
        'test5': 'aaa'
    },
    'keyAAA': {
        'test1': '2',
        'test2': 'bbb',
        'test3': 'aaa',
        'test4': 'aaa',
        'test5': { 'child': 'foo' }
    },
    'keyBBB': {
        'test1': '3',
        'test2': 'ccc',
        'test3': 'bbb',
        'test4': 'aaa',
        'test5': 'bbb'
    },
}
";

            RunTest(json, "test1", new[] {"keyXXX", "keyAAA", "keyBBB"});
            RunTest(json, "test2", new[] {"keyXXX", "keyAAA", "keyBBB"});
            RunTest(json, "test3", new[] {"keyAAA", "keyBBB", "keyXXX"});
            RunTest(json, "test4", new[] {"keyAAA", "keyBBB", "keyXXX"});
            RunTest(json, "test5", new[] {"keyXXX", "keyBBB", "keyAAA"});
        }

        [TestMethod]
        public void AllMixedTests()
        {
            string json = @"
{
    'keyAAA': {
        'test1': null,
        'test2': { 'child': 'foo' }
    },
    'keyBBB': {
        'test1': false,
        'test2': 'bbb'
    },
    'keyCCC': {
        'test1': true,
        'test2': 'aaa'
    },
    'keyDDD': {
        'test1': 1,
        'test2': 2.0
    },
    'keyEEE': {
        'test1': 2.0,
        'test2': 1
    },
    'keyFFF': {
        'test1': 'aaa',
        'test2': true
    },
    'keyGGG': {
        'test1': 'bbb',
        'test2': false
    },
    'keyHHH': {
        'test1': { 'child': 'foo' },
        'test2': null
    },
}
";

            RunTest(json, "test1",
                new[] {"keyAAA", "keyBBB", "keyCCC", "keyDDD", "keyEEE", "keyFFF", "keyGGG", "keyHHH"});
            RunTest(json, "test2",
                new[] {"keyHHH", "keyGGG", "keyFFF", "keyEEE", "keyDDD", "keyCCC", "keyBBB", "keyAAA",});
        }

        [TestMethod]
        public void ObjectTests()
        {
            string json = @"
{
    'keyXXX': {
        'test1': { 'child': 1 },
        'test2': { 'child': 1 }
    },
    'keyAAA': {
        'test1': { 'child': 2 },
        'test2': { 'child': 1 }
    },
    'keyBBB': {
        'test1': { 'child': 3 },
        'test2': null
    },
}
";

            RunTest(json, "test1", new[] {"keyAAA", "keyBBB", "keyXXX"});
            RunTest(json, "test2", new[] {"keyBBB", "keyAAA", "keyXXX"});
        }


        private void RunTest(string json, string testName, string[] expectedOrder)
        {
            using (FirebaseApp app = AppFactory.FromJson(json))
            {
                ManualResetEvent fired = new ManualResetEvent(false);
                var query = app.Child("/")
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