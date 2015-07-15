using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirebaseSharp.Tests
{
    [TestClass]
    public class FirebaseTests
    {
        [TestMethod]
        [ExpectedException(typeof(UriFormatException))]
        public void UriMustBeAbsolute_String()
        {
            Portable.Firebase fb = new Portable.Firebase("/relative/uri");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void UriMustBeAbsolute_Uri()
        {
            Portable.Firebase fb = new Portable.Firebase(new Uri("/relative/uri", UriKind.Relative));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UriMustNotBeNull_String()
        {
            string uri = null;
            Portable.Firebase fb = new Portable.Firebase(uri);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void UriMustNotBeNull_Uri()
        {
            Uri uri = null;
            Portable.Firebase fb = new Portable.Firebase(uri);
        }
    }
}
