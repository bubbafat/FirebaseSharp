using System;
using System.Diagnostics;
using System.Threading;
using FirebaseSharp.Portable;
using FirebaseSharp.Portable.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirebaseSharp.Tests.Filter
{
    [TestClass]
    public class TakeFirst
    {
        [TestMethod]
        public void TakesTwoDinosaurs()
        {
            using (FirebaseApp app = new FirebaseApp(new Uri("https://dinosaur-facts.firebaseio.com/")))
            {
                int limit = 2;
                int current = 0;

                ManualResetEvent fired = new ManualResetEvent(false);
                IFirebase limited = app.Child("/dinosaurs")
                    .LimitToFirst(limit)
                    .On("child_added", (snap, previous, context) =>
                    {
                        Debug.WriteLine(snap.Value);
                        if (++current == limit)
                        {
                            fired.Set();
                        }
                    });

                fired.WaitOne();
            }
        }
    }
}
