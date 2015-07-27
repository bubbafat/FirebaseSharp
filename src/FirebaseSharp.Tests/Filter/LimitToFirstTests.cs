using System;
using System.Diagnostics;
using System.Threading;
using FirebaseSharp.Portable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirebaseSharp.Tests.Filter
{
    [TestClass]
    public class LimitToFirstTests
    {
        [TestMethod]
        public void TakesTwoDinosaurs()
        {
            using (FirebaseApp app = AppFactory.Dinosaurs())
            {
                int limit = 2;
                int current = 0;

                ManualResetEvent fired = new ManualResetEvent(false);
                var query = app.Child("/dinosaurs")
                    .LimitToFirst(limit)
                    .On("child_added", (snap, previous, context) =>
                    {
                        Debug.WriteLine(snap.Value());
                        if (++current == limit)
                        {
                            fired.Set();
                        }
                    });

                Assert.IsTrue(fired.WaitOne(TimeSpan.FromSeconds(5)),
                    string.Format("callback did not fire enough times: {0}", current));
            }
        }
    }
}