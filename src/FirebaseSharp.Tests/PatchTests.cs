using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseSharp.Portable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace FirebaseSharp.Tests
{
    [TestClass]
    public class PatchTests
    {
        [TestMethod]
        public void PatchPatches()
        {
            FirebaseCache cache = new FirebaseCache();

            using (StringReader r = new StringReader("{\"a\": 1, \"b\": 2, \"c\": {\"foo\": true, \"bar\": false}}"))
            using (JsonReader reader = new JsonTextReader(r))
            {
                cache.Replace("/", reader);
            }

            using (StringReader r = new StringReader("{\"foo\": 3, \"baz\": 4}"))
            using (JsonReader reader = new JsonTextReader(r))
            {
                cache.Update("/c", reader);
            }


            /*
             * a = 1
             * b = 2
             * c = {
             *         foo = 3
             *         bar = false
             *         baz = 4
             *     }
             *     
             */
            Assert.IsTrue(cache.Root.Children.First(a => a.Name == "a").Value == "1");
            Assert.IsTrue(cache.Root.Children.First(a => a.Name == "b").Value == "2");
            CacheItem c = cache.Root.Children.First(a => a.Name == "c");

            Assert.IsTrue(c.Children.First(foo => foo.Name == "foo").Value == "3");
            Assert.IsTrue(c.Children.First(foo => foo.Name == "bar").Value == "False");
            Assert.IsTrue(c.Children.First(foo => foo.Name == "baz").Value == "4");
        }
    }
}
