using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using FirebaseSharp.Portable;
using FirebaseSharp.Portable.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Tests.Filter
{
    [TestClass]
    public class OrderByValue
    {
        [TestMethod]
        public void OrderedScores()
        {
            using (FirebaseApp app = AppFactory.Dinosaurs())
            {
                int previousScore = int.MinValue;
                int expect = 6;
                int index = 0;

                ManualResetEvent fired = new ManualResetEvent(false);
                IFirebase limited = app.Child("/scores")
                    .OrderByValue<int>()
                    .On("child_added", (snap, previous, context) =>
                    {
                        int current = snap.Value<int>();
                        Assert.IsTrue(previousScore < current, "items are out of order");
                        previousScore = current;


                        if (++index == expect)
                        {
                            fired.Set();
                        }
                    });

                Assert.IsTrue(fired.WaitOne(TimeSpan.FromSeconds(5)), "callback did not fire");
            }
        }
    }
}
