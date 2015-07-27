using System;
using System.Threading;
using FirebaseSharp.Portable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirebaseSharp.Tests.Firebase
{
    [TestClass]
    public class ConnectedTests
    {
        [TestMethod]
        public void InfoConnectedUpdated()
        {
            using (FirebaseApp app = AppFactory.Empty())
            {
                ManualResetEvent done = new ManualResetEvent(false);

                bool[] expected = new[] {true};

                app.Child("/.info/connected").On("value", (snap, child, context) =>
                {
                    Assert.AreEqual(expected[0], snap.Value<bool>());
                    done.Set();
                });

                Assert.IsTrue(done.WaitOne(TimeSpan.FromSeconds(5555)));

                done.Reset();
                expected[0] = false;
                app.GoOffline();
                Assert.IsTrue(done.WaitOne(TimeSpan.FromSeconds(5555)));
                
                done.Reset();
                expected[0] = true;
                app.GoOnline();
                Assert.IsTrue(done.WaitOne(TimeSpan.FromSeconds(5555)));
            }
        }
    }
}
