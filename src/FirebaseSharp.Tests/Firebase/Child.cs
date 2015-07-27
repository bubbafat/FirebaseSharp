using FirebaseSharp.Portable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirebaseSharp.Tests.Firebase
{
    [TestClass]
    public class Child
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
        public void SingleLevel_Exists()
        {
            _app.Child("foo").Set("{value: 'bar'}");
            Assert.AreEqual("foo", _app.Child("foo").Key);
        }

        [TestMethod]
        public void MultiLevel_Exists()
        {
            _app.Child("foo/bar/baz").Set("{value: 'bar'}");
            Assert.AreEqual("baz", _app.Child("foo/bar/baz").Key);
        }

        [TestMethod]
        public void SingleLevel_Missing()
        {
            _app.Child("foo").Set("{value: 'bar'}");
            Assert.AreEqual("foo", _app.Child("foo").Key);
        }

        [TestMethod]
        public void MultiLevel_Missing()
        {
            _app.Child("foo/bar/baz").Set("{value: 'bar'}");
            Assert.AreEqual("baz", _app.Child("foo/bar/baz").Key);
        }

        [TestMethod]
        public void Root()
        {
            Assert.AreEqual("/", _app.Child("/").Key);
        }

        [TestMethod]
        public void Empty()
        {
            Assert.AreEqual("/", _app.Child("").Key);
        }

        [TestMethod]
        public void Null()
        {
            Assert.AreEqual("/", _app.Child(null).Key);
        }
    }
}