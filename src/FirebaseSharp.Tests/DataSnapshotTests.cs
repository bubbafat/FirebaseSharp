using System;
using System.Linq;
using System.Threading;
using FirebaseSharp.Portable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirebaseSharp.Tests
{
    [TestClass]
    public class DataSnapshotTests
    {
        [TestMethod]
        public void ChildPathTests()
        {
            string json = @"
{
    'foo': {
        'bar': {
            'baz': {
            }
        }
    }
}
";
            using (FirebaseApp app = AppFactory.FromJson(json))
            {
                ManualResetEvent done = new ManualResetEvent(false);

                var query = app.Child("foo/bar/baz");
                query.On("value", (snap, child, context) =>
                {
                    Assert.AreEqual("baz", snap.Key);
                    Assert.AreEqual("baz", snap.Ref().Key);

                    // new ref should be at
                    // http://<app root>/foo/bar/baz
                    Assert.AreEqual(
                        string.Format("{0}{1}", app.RootUri, "foo/bar/baz"),
                        snap.Ref().AbsoluteUri.ToString());

                    var pipo = snap.Child("pipo");

                    Assert.AreEqual("pipo", pipo.Key);
                    Assert.AreEqual("pipo", pipo.Ref().Key);

                    Assert.AreEqual(
                        string.Format("{0}{1}", app.RootUri, "foo/bar/baz/pipo"),
                        pipo.Ref().AbsoluteUri.ToString());

                    var bar3 = pipo.Child("bar1/bar2/bar3");

                    Assert.AreEqual("bar3", bar3.Key);
                    Assert.AreEqual("bar3", bar3.Ref().Key);

                    Assert.AreEqual(
                        string.Format("{0}{1}", app.RootUri, "foo/bar/baz/pipo/bar1/bar2/bar3"),
                        bar3.Ref().AbsoluteUri.ToString());


                    done.Set();
                });

                Assert.IsTrue(done.WaitOne(TimeSpan.FromSeconds(5)), "The callback never fired");
            }
        }

        [TestMethod]
        public void ChildrenValues()
        {
            string json = @"
{
  'messages': {
    '-JqpIO567aKezufthrn8': {
      'uid': 'barney',
      'text': 'Welcome to Bedrock City!'
    },
      '-JqpIP5tIy-gMbdTmIg7': {
      'uid': 'fred',
      'text': 'Yabba dabba doo!'
    }
  }
}

";

            using (FirebaseApp app = AppFactory.FromJson(json))
            {
                ManualResetEvent done = new ManualResetEvent(false);

                var query = app.Child("messages");
                query.Once("value", (snap, child, context) =>
                {
                    var ca = snap.Children.ToArray();
                    Assert.AreEqual("-JqpIO567aKezufthrn8", ca[0].Key);
                    Assert.AreEqual("barney", ca[0].Child("uid").Value());
                    Assert.AreEqual("Welcome to Bedrock City!", ca[0].Child("text").Value());

                    Assert.AreEqual("-JqpIP5tIy-gMbdTmIg7", ca[1].Key);
                    Assert.AreEqual("fred", ca[1].Child("uid").Value());
                    Assert.AreEqual("Yabba dabba doo!", ca[1].Child("text").Value());

                    done.Set();
                });

                Assert.IsTrue(done.WaitOne(TimeSpan.FromSeconds(5)), "callback never fired");
            }
        }
    }
}