using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebaseSharp.Portable;

namespace HackerNewsUpdatesCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            // 


            using (FirebaseApp app = new FirebaseApp(new Uri("https://hacker-news.firebaseio.com/v0/updates")))
            {
                var scoresRef = app.Child("profiles")
                                   .On("value",
                    (snapshot, child, context) =>
                    {
                        Console.WriteLine("{0} - {1}", snapshot.Key, snapshot.Value());
                    });

                Thread.Sleep(TimeSpan.FromMinutes(5));
            }
        }
    }
}
