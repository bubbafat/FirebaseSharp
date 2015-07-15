using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ShallowReads
{
    class Program
    {
        static void Main(string[] args)
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 10,

            };

            HttpClient _client = new HttpClient(handler, true)
            {
                BaseAddress = new Uri("http://www.weather.com"),
                Timeout = TimeSpan.FromMinutes(1),
            };

            Uri uri = new Uri("http://www.weather.com/weather/today/l/USNC0326:1:US");


            List<long> averages = new List<long>();

            for (int i = 0; i < 20; i++)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
                // request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

                using (
                    HttpResponseMessage response =
                        _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).Result)
                {
                    response.EnsureSuccessStatusCode();

                    using (var content = response.Content.ReadAsStreamAsync().Result)
                    using (StreamReader sr = new StreamReader(content))
                    {
                        while (true)
                        {
                            string read = sr.ReadLineAsync().Result;
                            if (read != null)
                            {
                                Console.WriteLine(read);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                sw.Stop();
                averages.Add(sw.ElapsedMilliseconds);
            }

            Console.WriteLine("Min: {0}", averages.Min());
            Console.WriteLine("Max: {0}", averages.Max());
            Console.WriteLine("Average: {0}", averages.Average());
        }
    }
}
