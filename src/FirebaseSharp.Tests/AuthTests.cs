using Firebase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;

namespace FirebaseSharp.Tests
{
    [TestClass]
    public class AuthTests
    {
        [TestClass]
        public class AuthTest
        {
            [TestMethod]
            public void ReadAuthFails()
            {
                Portable.Firebase firebase = new Portable.Firebase(TestConfig.RootUrl);
                try
                {
                    firebase.Get("sec/no/shouldFail");
                    Assert.Fail("Should have thrown HTTP 401 error");
                }
                catch (AggregateException ex)
                {
                    Exception cur = ex;
                    while (cur != null)
                    {
                        if (cur is HttpRequestException && cur.Message.Contains("401"))
                        {
                            // we found our auth exception
                            return;
                        }

                        cur = cur.InnerException;
                    }

                    throw;
                }
            }

            [TestMethod]
            public void ReadAuthPasses()
            {
                string authToken = getToken();
                Portable.Firebase firebase = new Portable.Firebase(TestConfig.RootUrl, authToken);

                firebase.Get("sec/no/shouldPass");
            }

            private string getToken()
            {
                var tokenGenerator = new TokenGenerator(TestConfig.FirebaseSecret);
                return tokenGenerator.CreateToken(null, new TokenOptions(admin: true));
            }

        }
    }
}
