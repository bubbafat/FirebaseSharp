using FirebaseSharp.Portable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirebaseSharp.Tests.Firebase
{
    [TestClass]
    public class ToString
    {
        private IFirebaseApp _app;

        [TestInitialize]
        public void TestInit()
        {
            // sets root URL to https://example.com/
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
        public void SingleChild()
        {
            Assert.AreEqual("https://example.com/foo", _app.Child("foo").ToString());
            Assert.AreEqual("https://example.com/foo", _app.Child("/foo").ToString());
            Assert.AreEqual("https://example.com/foo", _app.Child("foo/").ToString());
            Assert.AreEqual("https://example.com/foo", _app.Child("/foo/").ToString());
        }

        [TestMethod]
        public void MultiChild()
        {
            Assert.AreEqual("https://example.com/foo/bar/baz", _app.Child("foo/bar/baz").ToString());
            Assert.AreEqual("https://example.com/foo/bar/baz", _app.Child("/foo/bar/baz").ToString());
            Assert.AreEqual("https://example.com/foo/bar/baz", _app.Child("foo/bar/baz/").ToString());
            Assert.AreEqual("https://example.com/foo/bar/baz", _app.Child("/foo/bar/baz/").ToString());
        }

        [TestMethod]
        public void Root()
        {
            Assert.AreEqual("https://example.com/", _app.Child("/").ToString());
            Assert.AreEqual("https://example.com/", _app.Child("//").ToString());
            Assert.AreEqual("https://example.com/", _app.Child("").ToString());
            Assert.AreEqual("https://example.com/", _app.Child(null).ToString());
        }
    }
}