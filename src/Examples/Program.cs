using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebaseSharp.Portable;

namespace Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            using (FirebaseApp app = new FirebaseApp(new Uri("https://docs-examples.firebaseio.com/rest/saving-data/fireblog/users")))
            {
                app.Child("gracehop").Once("value", (snap, child, context) =>
                {
                    Console.WriteLine(snap.Value());
                });

                Thread.Sleep(TimeSpan.FromSeconds(5));
            }

            TopThreeDinos.StartAtFilter();
        }
    }
}
