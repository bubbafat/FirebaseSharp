using Firebase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace FirebaseSharp.Tests
{
    [TestClass]
    public class AuthTests
    {
        [TestMethod]
        public async Task ReadAuthFails()
        {
            Portable.Firebase firebase = new Portable.Firebase(TestConfig.RootUrl);
            try
            {
                await firebase.GetAsync("sec/no/shouldFail");
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
            catch (HttpRequestException ex)
            {
                if (!ex.Message.Contains("401"))
                {
                    throw;
                }
            }
        }

        [TestMethod]
        public async Task ReadAuthPasses()
        {
            string authToken = getToken();
            Portable.Firebase firebase = new Portable.Firebase(TestConfig.RootUrl, authToken);
            await firebase.GetAsync("sec/no/shouldPass");
        }

        private string getToken()
        {
            var tokenGenerator = new TokenGenerator(TestConfig.FirebaseSecret);
            return tokenGenerator.CreateToken(null, new TokenOptions(admin: true));
        }

    }
}
