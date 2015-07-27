using FirebaseSharp.Portable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirebaseSharp.Tests.Firebase
{
    [TestClass]
    public class Root
    {
        private IFirebaseApp _app;

        [TestInitialize]
        public void TestInit()
        {
            _app = AppFactory.Empty();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            using (_app)
            {
            }
        }

        [TestMethod]
        public void MultiChild()
        {
            Assert.AreEqual("/", _app.Child("foo/bar/baz").Root().Key);
        }

        [TestMethod]
        public void SingleChild()
        {
            Assert.AreEqual("/", _app.Child("foo").Root().Key);
        }

        [TestMethod]
        public void RootChild()
        {
            Assert.AreEqual("/", _app.Child("/").Root().Key);
        }
    }
}