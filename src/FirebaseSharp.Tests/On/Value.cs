using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirebaseSharp.Tests.On
{
    [TestClass]
    public class Value
    {
        // https://www.firebase.com/docs/web/guide/retrieving-data.html#section-event-guarantees
        // value event is always fired after the other events

        [TestMethod]
        public void ValueFiresLast()
        {
            string json = @"
{
    child1: { value: 1 },
    child2: { value: 1 },
    child3: { value: 1 },
    child4: { value: 1 },
    child5: { value: 1 }
}
";
            using (var app = AppFactory.FromJson(json))
            {
                ManualResetEvent done= new ManualResetEvent(false);
                int count = 0;
                var query = app.Child("/");
                query.On("child_added", (snap, child, context) =>
                {
                    count++;
                });

                query.Once("value", (snap, child, context) =>
                {
                    Assert.AreEqual(count, snap.NumChildren);
                    done.Set();
                });

                Assert.IsTrue(done.WaitOne(TimeSpan.FromSeconds(5)), "callback never fired");
                Assert.AreEqual(5, count);

            }
        }
    }
}
