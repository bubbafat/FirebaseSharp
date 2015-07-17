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
            FirebaseApp app = new FirebaseApp(new Uri("https://dinosaur-facts.firebaseio.com/"));            

            ManualResetEvent fired = new ManualResetEvent(false);
            IFirebase limited = app.Child("/dinosaurs").LimitToFirst(2).Once("child_added", (snap, previous, context) =>
            {
                Debug.WriteLine(snap.Value);
                fired.Set();
            });

            fired.WaitOne();
        }
    }
}
