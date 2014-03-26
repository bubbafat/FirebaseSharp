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
        private const string RootUrl = "https://dazzling-fire-1575.firebaseio.com/";

        [TestMethod]
        public void EndToEnd()
        {
            Firebase fb = new Firebase(new Uri(RootUrl));

            string testRoot = string.Format("/test/{0}", DateTime.UtcNow.Ticks);

            List<StreamingEvent> callbackResults = new List<StreamingEvent>();
            List<string> created = new List<string>();

            ManualResetEvent received = new ManualResetEvent(false);

            using (fb.GetStreaming(testRoot, response =>
            {
                callbackResults.Add(response);
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

                var found = callbackResults.First(c => c.Payload.Contains(key));
                Assert.IsNotNull(found, "The key was added but missing from stream");

                dynamic payload = JsonConvert.DeserializeObject(found.Payload);

                string singlePath = testRoot + payload.path.ToString();

                string single = fb.Get(singlePath);

                dynamic payloadSingle = JsonConvert.DeserializeObject(single);

                Assert.AreEqual(payload.data.value.ToString(), payloadSingle.value.ToString());

                fb.Delete(singlePath);

                string missing = fb.Get(singlePath);
                Assert.AreEqual("null", missing);
            }

            fb.Delete(testRoot);

        }
    }
}
