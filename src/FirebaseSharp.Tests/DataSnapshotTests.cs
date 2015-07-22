using System;
using System.Threading;
using FirebaseSharp.Portable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirebaseSharp.Tests
{
    [TestClass]
    public class DataSnapshotTests
    {
        [TestMethod]
        public void ChildPathTests()
        {
            string json = @"
{
    'foo': {
        'bar': {
            'baz': {
            }
        }
    }
}
";
            using (FirebaseApp app = AppFactory.FromJson(json))
            {
                ManualResetEvent done= new ManualResetEvent(false);

                var query = app.Child("foo/bar/baz");
                query.On("value", (snap, child, context) =>
                {
                    Assert.AreEqual("baz", snap.Key);
                    Assert.AreEqual("baz", snap.Ref().Key);
                    done.Set();
                });

                Assert.IsTrue(done.WaitOne(TimeSpan.FromSeconds(5)), "The callback never fired");
            }
        }
    }
}
