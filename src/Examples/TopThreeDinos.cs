using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebaseSharp.Portable;

namespace Examples
{
    static class TopThreeDinos
    {
        public static void Run()
        {
            /* Node:
             * 
             * var scoresRef = new Firebase("https://dinosaur-facts.firebaseio.com/scores");
             * scoresRef.orderByValue().limitToLast(3).on("value", function(snapshot) {
               * snapshot.forEach(function(data) {
             *     console.log("The " + data.key() + " dinosaur's score is " + data.val());
             *   });
             * });
             * 
             */

            ManualResetEvent done = new ManualResetEvent(false);

            using (FirebaseApp app = new FirebaseApp(new Uri("https://dinosaur-facts.firebaseio.com/")))
            {
                var scoresRef = app.Child("scores").OrderByValue<int>().LimitToLast(3).On("value",
                    (snapshot, child, context) =>
                    {
                        foreach (var data in snapshot.Children)
                        {
                            Console.WriteLine("The {0} dinosaur\'s score is {1}", data.Key, data.Value<int>());
                        }

                        done.Set();
                    });

                done.WaitOne(TimeSpan.FromSeconds(15));
            }
        }
    }
}
