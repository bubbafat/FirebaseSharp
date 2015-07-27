using System;
using System.Linq;
using System.Threading;
using FirebaseSharp.Portable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirebaseSharp.Tests.Filter
{
    [TestClass]
    public class EqualTo
    {
        [TestMethod]
        public void Basic()
        {
            string json = @"
{
    aaa: {
        test: 1
    },
    bbb: {
        test: '1'
    },
    ccc: {
        test: 3
    },
    ddd: {
    },
    eee: {
        test: {
            other: 'value'
        }
    },
    fff: 'value'
}
";

            using (FirebaseApp app = AppFactory.FromJson(json))
            {
                ManualResetEvent done = new ManualResetEvent(false);

                app.Child("/").OrderByChild("test").EqualTo(3).Once("value", (snap, child, context) =>
                {
                    Assert.AreEqual(1, snap.NumChildren);
                    Assert.AreEqual("ccc", snap.Children.First().Key);
                    done.Set();
                });

                Assert.IsTrue(done.WaitOne(TimeSpan.FromSeconds(5)));
            }
        }
    }
}