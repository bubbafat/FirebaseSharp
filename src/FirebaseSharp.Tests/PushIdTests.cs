using FirebaseSharp.Portable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirebaseSharp.Tests
{
    [TestClass]
    public class PushIdTests
    {
        [TestMethod]
        public void IncrementTests()
        {
            FirebasePushIdGenerator gen = new FirebasePushIdGenerator();
            string last = gen.Next();
            Assert.AreEqual(20, last.Length);

            for (int i = 0; i < 10000; i++)
            {
                string current = gen.Next();

                Assert.AreEqual(last.Length, current.Length);
                Assert.IsTrue(string.CompareOrdinal(last, current) < 0);

                last = current;
            }
        }
    }
}