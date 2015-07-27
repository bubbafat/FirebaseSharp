using System;
using System.Linq;
using System.Threading;
using FirebaseSharp.Portable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirebaseSharp.Tests
{
    [TestClass]
    public class PriorityTests
    {
        [TestMethod]
        public void OrderByPriorityFloat()
        {
            string json = @"
{  
    'users':{  
        'gracehop':{  
            '.priority':1.5,
            'name':'Grace Hopper'
        },
        'physicsmarie':{  
            '.priority':1.0,
            'name':'Marie Curie'
        },
        'adalovelave':{  
            '.priority':2.0,
            'name':'Ada Lovelace'
        },
    }
}
";

            using (FirebaseApp app = AppFactory.FromJson(json))
            {
                ManualResetEvent done = new ManualResetEvent(false);

                var root = app.Child("users");
                var query = root.OrderByPriority().Once("value", (snap, child, context) =>
                {
                    var list = snap.Children.ToList();
                    Assert.AreEqual("physicsmarie", list[0].Key);
                    Assert.AreEqual("gracehop", list[1].Key);
                    Assert.AreEqual("adalovelave", list[2].Key);

                    done.Set();
                });

                Assert.IsTrue(done.WaitOne(TimeSpan.FromSeconds(10)), "callback did not fire");
            }
        }

        [TestMethod]
        public void OrderByPriorityString()
        {
            string json = @"
{  
    'users':{  
        'gracehop':{  
            '.priority': 'aaa',
            'name':'Grace Hopper'
        },
        'physicsmarie':{  
            '.priority': 'bbb',
            'name':'Marie Curie'
        },
        'adalovelave':{  
            '.priority': 'ccc',
            'name':'Ada Lovelace'
        },
    }
}
";

            using (FirebaseApp app = AppFactory.FromJson(json))
            {
                ManualResetEvent done = new ManualResetEvent(false);

                var root = app.Child("users");
                var query = root.OrderByPriority().Once("value", (snap, child, context) =>
                {
                    var list = snap.Children.ToList();
                    Assert.AreEqual("gracehop", list[0].Key);
                    Assert.AreEqual("physicsmarie", list[1].Key);
                    Assert.AreEqual("adalovelave", list[2].Key);

                    done.Set();
                });

                Assert.IsTrue(done.WaitOne(TimeSpan.FromSeconds(10)), "callback did not fire");
            }
        }

        [TestMethod]
        public void OrderByPriorityStringNumerics()
        {
            string json = @"
{  
    'users':{  
        'gracehop':{  
            '.priority': '1',
            'name':'Grace Hopper'
        },
        'physicsmarie':{  
            '.priority': 2,
            'name':'Marie Curie'
        },
        'adalovelave':{  
            '.priority': '3',
            'name':'Ada Lovelace'
        },
    }
}
";

            using (FirebaseApp app = AppFactory.FromJson(json))
            {
                ManualResetEvent done = new ManualResetEvent(false);

                var root = app.Child("users");
                var query = root.OrderByPriority().Once("value", (snap, child, context) =>
                {
                    var list = snap.Children.ToList();
                    Assert.AreEqual("gracehop", list[0].Key);
                    Assert.AreEqual("physicsmarie", list[1].Key);
                    Assert.AreEqual("adalovelave", list[2].Key);

                    done.Set();
                });

                Assert.IsTrue(done.WaitOne(TimeSpan.FromSeconds(10)), "callback did not fire");
            }
        }

        [TestMethod]
        public void OrderByPriorityNulls()
        {
            string json = @"
{  
    'users':{  
        'gracehop':{  
            '.priority': '1',
            'name':'Grace Hopper'
        },
        'physicsmarie':{  
            'name':'Marie Curie'
        },
        'adalovelave':{  
            'name':'Ada Lovelace'
        },
    }
}
";

            using (FirebaseApp app = AppFactory.FromJson(json))
            {
                ManualResetEvent done = new ManualResetEvent(false);

                var root = app.Child("users");
                var query = root.OrderByPriority().Once("value", (snap, child, context) =>
                {
                    var list = snap.Children.ToList();
                    Assert.AreEqual("adalovelave", list[0].Key);
                    Assert.AreEqual("physicsmarie", list[1].Key);
                    Assert.AreEqual("gracehop", list[2].Key);

                    done.Set();
                });

                Assert.IsTrue(done.WaitOne(TimeSpan.FromSeconds(10)), "callback did not fire");
            }
        }

        [TestMethod]
        public void OrderByPriorityMixed()
        {
            string json = @"
{  
    'users':{  
        'aaa':{  
            '.priority': 5,
            'name':'AAA'
        },
        'bbb':{  
            '.priority': 'qbert',
            'name':'BBB'
        },
        'ccc':{  
            '.priority': 'zzyzx',
            'name':'CCC'
        },
        'ddd':{  
            'name':'DDD'
        },
        'eee':{  
            '.priority': '3',
            'name':'EEE'
        },
    }
}
";

            using (FirebaseApp app = AppFactory.FromJson(json))
            {
                ManualResetEvent done = new ManualResetEvent(false);

                var root = app.Child("users");
                var query = root.OrderByPriority().Once("value", (snap, child, context) =>
                {
                    var list = snap.Children.ToList();
                    Assert.AreEqual("ddd", list[0].Key);
                    Assert.AreEqual("eee", list[1].Key);
                    Assert.AreEqual("aaa", list[2].Key);
                    Assert.AreEqual("bbb", list[3].Key);
                    Assert.AreEqual("ccc", list[4].Key);

                    done.Set();
                });

                Assert.IsTrue(done.WaitOne(TimeSpan.FromSeconds(10)), "callback did not fire");
            }
        }

        [TestMethod]
        public void SetPriorityTest()
        {
            string json = @"
{
    'aaa': {
    },
    'bbb': {
    },
    'ccc': {
    },
}
";

            using (var app = AppFactory.FromJson(json))
            {
                ManualResetEvent called = new ManualResetEvent(false);

                var root = app.Child("/");

                // check start stat
                root.OrderByPriority().Once("value", (snap, child, context) =>
                {
                    var children = snap.Children.ToArray();
                    Assert.AreEqual("aaa", children[0].Key);
                    Assert.AreEqual("bbb", children[1].Key);
                    Assert.AreEqual("ccc", children[2].Key);
                    called.Set();
                });

                Assert.IsTrue(called.WaitOne(TimeSpan.FromSeconds(5)), "callback was never fired");

                // now update the priorites
                root.Child("aaa").SetPriority(3);
                root.Child("bbb").SetPriority(2);
                root.Child("ccc").SetPriority(1);

                called.Reset();
                // check start stat
                root.OrderByPriority().Once("value", (snap, child, context) =>
                {
                    var children = snap.Children.ToArray();
                    Assert.AreEqual("ccc", children[0].Key);
                    Assert.AreEqual("bbb", children[1].Key);
                    Assert.AreEqual("aaa", children[2].Key);
                    called.Set();
                });

                Assert.IsTrue(called.WaitOne(TimeSpan.FromSeconds(5)), "callback was never fired");
            }
        }

        [TestMethod]
        public void SetWithPriorityTest()
        {
            using (var app = AppFactory.Empty())
            {
                ManualResetEvent called = new ManualResetEvent(false);

                var root = app.Child("/");

                // now update the priorites
                root.Child("aaa").SetWithPriority("{ name: 'aaa'}", 3);
                root.Child("bbb").SetWithPriority("{ name: 'bbb'}", 2);
                root.Child("ccc").SetWithPriority("{ name: 'ccc'}", 1);

                root.OrderByPriority().Once("value", (snap, child, context) =>
                {
                    var children = snap.Children.ToArray();
                    Assert.AreEqual("ccc", children[0].Key);
                    Assert.AreEqual(1, float.Parse(children[0].GetPriority().Value));
                    Assert.AreEqual("bbb", children[1].Key);
                    Assert.AreEqual(2, float.Parse(children[1].GetPriority().Value));
                    Assert.AreEqual("aaa", children[2].Key);
                    Assert.AreEqual(3, float.Parse(children[2].GetPriority().Value));
                    called.Set();
                });

                Assert.IsTrue(called.WaitOne(TimeSpan.FromSeconds(5)), "callback was never fired");
            }
        }
    }
}