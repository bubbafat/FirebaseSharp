using FirebaseSharp.Portable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;

namespace FirebaseSharp.Tests
{
    [TestClass]
    public class EndToEndLiveTest
    {
        [TestMethod]
        public void EndToEnd()
        {
            Portable.Firebase fb = new Portable.Firebase(new Uri(TestConfig.RootUrl));

            string testRoot = string.Format("/test/{0}", DateTime.UtcNow.Ticks);

            List<ValueAddedEventArgs> callbackResults = new List<ValueAddedEventArgs>();
            List<string> created = new List<string>();

            ManualResetEvent received = new ManualResetEvent(false);

            using (fb.GetStreaming(testRoot, added: (sender, args) =>
            {
                callbackResults.Add(args);
                received.Set();
            }))
            {
                for (int i = 0; i < 10; i++)
                {
                    created.Add(fb.Post(testRoot, string.Format("{{\"value\": \"{0}\"}}", i)));
                    received.WaitOne();
                    received.Reset();
                }
            }

            // now go take everything we created and load it from our
            // stream gets.  Read the values out and then re-fetch them
            // individually.  Compare the values to make sure they match.
            // Then delete the individual node and perform another
            // get to make sure it is missing.

            foreach (string keyResponse in created)
            {
                dynamic keyObj = JsonConvert.DeserializeObject(keyResponse);
                string key = keyObj.name;

                var found = callbackResults.First(c => c.Path.Contains(key));
                Assert.IsNotNull(found, "The key was added but missing from stream");

                string singlePath = testRoot + found.Path;

                string single = fb.Get(singlePath);

                dynamic payloadSingle = JsonConvert.DeserializeObject(single);

                Assert.AreEqual(found.Data, payloadSingle);

                fb.Delete(singlePath);

                string missing = fb.Get(singlePath);
                Assert.AreEqual("null", missing);
            }

            fb.Delete(testRoot);

        }
    }
}
