using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebaseSharp.Portable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Tests.JsonCache 
{
    [TestClass]
    public class IntegrityTests
    {
        private readonly string _weather;
        public IntegrityTests()
        {
            _weather = LoadData("weather.json");
        }

        private string LoadData(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var name = assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith(string.Format(".{0}", fileName)));

            using (Stream stream = assembly.GetManifestResourceStream(name))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }

            Assert.Fail("Resource not found: {0}", fileName);
            throw new Exception("Resource not found: " + fileName);
        }


        [TestMethod]
        public void PutMatchesInput()
        {
            JToken expected = JToken.Parse(_weather);
            var jc = new Portable.JsonCache();

            bool called = false;

            jc.Changed += (sender, args) =>
            {
                JToken actual = JToken.Parse(args.Data);
                Assert.IsTrue(JToken.DeepEquals(expected, actual));
                called = true;
            };

            jc.Put(ChangeSource.Local, "/", _weather);

            Assert.IsTrue(called, "Changed event never fired");
        }

        [TestMethod]
        public void PatchMatchesInput()
        {
            JToken expected = JToken.Parse(_weather);
            var jc = new Portable.JsonCache();

            bool called = false;

            jc.Changed += (sender, args) =>
            {
                JToken actual = JToken.Parse(args.Data);
                Assert.IsTrue(JToken.DeepEquals(expected, actual));
                called = true;
            };

            jc.Patch(ChangeSource.Local, "/", _weather);

            Assert.IsTrue(called, "Changed event never fired");
        }

        [TestMethod]
        public void MultiPatchMatchesOutput()
        {
            var data = new List<Tuple<Tuple<string, string>, string>>()
            {
                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/me",
                        "{\"name\": \"Robert\"}"),
                    "{\"people\": { \"me\": {\"name\": \"Robert\"}}}"),

                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/me/name",
                        "Bob"),
                    "{\"people\": { \"me\": {\"name\": \"Bob\"}}}"),

                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/you",
                        "{\"name\": \"Susan\"}"),
                    "{\"people\": { \"me\": {\"name\": \"Bob\"}, \"you\": {\"name\": \"Susan\"}}}"),

                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/me/age",
                        "38"),
                    "{\"people\": { \"me\": {\"name\": \"Bob\", \"age\": \"38\"}, \"you\": {\"name\": \"Susan\"}}}"),

                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/me",
                        "{\"name\": \"Bobby\"}"),
                    "{\"people\": { \"me\": {\"name\": \"Bobby\", \"age\": \"38\"}, \"you\": {\"name\": \"Susan\"}}}"),

                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/",
                        "{ \"me\": {\"name\": \"Bobert\"}}"),
                    "{\"people\": { \"me\": {\"name\": \"Bobert\" }, \"you\": {\"name\": \"Susan\"}}}"),

                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/me",
                        null),
                    "{\"people\": { \"you\": {\"name\": \"Susan\"}}}"),

            };
            
            var jc = new Portable.JsonCache();

            int[] current = new[]{0};
            bool[] called = new bool[data.Count];

            jc.Changed += (sender, args) =>
            {
                JToken actual = JToken.Parse(jc.Dump());
                JToken expected = JToken.Parse(data[current[0]].Item2);
                Assert.IsTrue(JToken.DeepEquals(expected, actual));
                called[current[0]] = true;
            };

            for (current[0] = 0; current[0] < data.Count; current[0]++)
            {
                jc.Patch(ChangeSource.Local, data[current[0]].Item1.Item1, data[current[0]].Item1.Item2);
            }

            foreach (bool did in called)
            {
                Assert.IsTrue(did, "At least one of the Changed events never fired");
            }
        }

        [TestMethod]
        public void MultiPutMatchesOutput()
        {
            var data = new List<Tuple<Tuple<string, string>, string>>()
            {
                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/me",
                        "{\"name\": \"Robert\"}"),
                    "{\"people\": { \"me\": {\"name\": \"Robert\"}}}"),

                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/me/name",
                        "Bob"),
                    "{\"people\": { \"me\": {\"name\": \"Bob\"}}}"),

                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/you",
                        "{\"name\": \"Susan\"}"),
                    "{\"people\": { \"me\": {\"name\": \"Bob\"}, \"you\": {\"name\": \"Susan\"}}}"),

                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/me/age",
                        "38"),
                    "{\"people\": { \"me\": {\"name\": \"Bob\", \"age\": \"38\"}, \"you\": {\"name\": \"Susan\"}}}"),

                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/me",
                        "{\"name\": \"Bobby\"}"),
                    "{\"people\": { \"me\": {\"name\": \"Bobby\" }, \"you\": {\"name\": \"Susan\"}}}"),

                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/",
                        "{ \"me\": {\"name\": \"Bobert\"}}"),
                    "{\"people\": { \"me\": {\"name\": \"Bobert\" }}}"),

                new Tuple<Tuple<string, string>, string>(
                    new Tuple<string, string>(
                        "/people/me/name",
                        null),
                    "{\"people\": { \"me\": {}}}"),

            };

            var jc = new Portable.JsonCache();

            int[] current = new[] { 0 };
            bool[] called = new bool[data.Count];

            jc.Changed += (sender, args) =>
            {
                JToken actual = JToken.Parse(jc.Dump());
                JToken expected = JToken.Parse(data[current[0]].Item2);
                Assert.IsTrue(JToken.DeepEquals(expected, actual));
                called[current[0]] = true;
            };

            for (current[0] = 0; current[0] < data.Count; current[0]++)
            {
                jc.Put(ChangeSource.Local, data[current[0]].Item1.Item1, data[current[0]].Item1.Item2);
            }

            foreach (bool did in called)
            {
                Assert.IsTrue(did, "At least one of the Changed events never fired");
            }
        }

    }
}
