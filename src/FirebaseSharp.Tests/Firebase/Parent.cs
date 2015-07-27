using FirebaseSharp.Portable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirebaseSharp.Tests.Firebase
{
    [TestClass]
    public class Parent
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
        public void ChildGivesParent_Exists()
        {
            _app.Child("foo/bar/baz").Set("{ value: 'val1'}");
            Assert.AreEqual("bar", _app.Child("foo/bar/baz").Parent().Key);
        }

        [TestMethod]
        public void ChildGivesParent_Missing()
        {
            Assert.AreEqual("bar", _app.Child("foo/bar/baz").Parent().Key);
        }

        [TestMethod]
        public void Root()
        {
            Assert.IsNull(_app.Child("/").Parent());
        }
    }
}