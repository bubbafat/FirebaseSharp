using System;
using System.Diagnostics;
using System.Threading;
using FirebaseSharp.Portable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirebaseSharp.Tests.Filter
{
    [TestClass]
    public class OrderByKeyTests
    {
        [TestMethod]
        public void OrderedDinos()
        {
            using (FirebaseApp app = AppFactory.Dinosaurs())
            {
                const int expected = 6;
                int current = 0;

                string previousKey = null;

                ManualResetEvent fired = new ManualResetEvent(false);
                var query = app.Child("/dinosaurs")
                    .OrderByKey()
                    .On("child_added", (snap, previous, context) =>
                    {
                        Debug.WriteLine(snap.Value());

                        Assert.IsNotNull(snap.Key);

                        if (previousKey != null)
                        {
                            Assert.IsTrue(String.Compare(previousKey, snap.Key, StringComparison.Ordinal) < 0);
                        }

                        previousKey = snap.Key;

                        if (++current == expected)
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