using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FirebaseSharp.Portable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirebaseSharp.Tests.Filter
{
    [TestClass]
    public class CombinedFilterTests
    {
        [TestMethod]
        public void OrderedAndLimited()
        {
            using (FirebaseApp app = AppFactory.Dinosaurs())
            {
                List<Tuple<string, int>> expected = new List<Tuple<string, int>>
                {
                    new Tuple<string, int>("bruhathkayosaurus", 55),
                    new Tuple<string, int>("linhenykus", 80),
                    new Tuple<string, int>("pterodactyl", 93),
                };

                List<Tuple<string, int>> actual = new List<Tuple<string, int>>();

                ManualResetEvent loaded = new ManualResetEvent(false);

                var fbRef = app.Child("/scores")
                    .OrderByValue()
                    .LimitToLast(expected.Count)
                    .On("value", (snap, childPath, context) =>
                    {
                        Assert.IsTrue(snap.HasChildren);
                        Assert.AreEqual(expected.Count, snap.NumChildren);
                        actual.AddRange(
                            snap.Children.Select(child => new Tuple<string, int>(child.Key, child.Value<int>())));
                        loaded.Set();
                    });

                Assert.IsTrue(loaded.WaitOne(TimeSpan.FromSeconds(5)), "Failed to load snapshot");

                Assert.AreEqual(expected.Count, actual.Count);

                for (int i = 0; i < expected.Count; i++)
                {
                    Assert.AreEqual(expected[i].Item1, actual[i].Item1);
                    Assert.AreEqual(expected[i].Item2, actual[i].Item2);
                }
            }
        }
    }
}