using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FirebaseSharp.Tests
{
    internal static class Extensions
    {
        public static bool Matches(this HttpRequestMessage req, HttpMethod method, Uri uri)
        {
            return req.RequestUri == uri &&
                   req.Method == method;
        }

        public static bool Matches(this HttpRequestMessage req, HttpMethod method, Uri uri, string content)
        {
            return req.RequestUri == uri &&
                   req.Method == method &&
                   req.Content.ReadAsStringAsync().Result == content;
        }

        public static bool MatchStreaming(this HttpRequestMessage req, HttpMethod method, Uri uri, string headerAccept)
        {
            bool matched = req.RequestUri == uri &&
                   req.Method == method &&
                   req.Headers.Accept.Any(h => h.MediaType == "text/event-stream");

            return matched;
        }

    }
}
