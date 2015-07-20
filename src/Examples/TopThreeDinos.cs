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
        public static void ByScore()
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

        public static void ByHeightChild()
        {
            /* Node:
             * 
             * var ref = new Firebase("https://dinosaur-facts.firebaseio.com/dinosaurs");
             * ref.orderByChild("height").on("child_added", function(snapshot) {
              *  console.log(snapshot.key() + " was " + snapshot.val().height + " meters tall");
             * });             
             */

            ManualResetEvent done = new ManualResetEvent(false);
            int got = 0;

            using (FirebaseApp app = new FirebaseApp(new Uri("https://dinosaur-facts.firebaseio.com/")))
            {
                var scoresRef = app.Child("dinosaurs").OrderByChild("height").On("child_added",
                    (snapshot, child, context) =>
                    {
                        Console.WriteLine("{0} was {1} meters tall", snapshot.Key, snapshot["height"].Value<float>());

                        if (++got == 6)
                        {
                            done.Set();
                        }
                    });

                done.WaitOne(TimeSpan.FromSeconds(15));
            }
        }

        public static void ByHeightFilter()
        {
            //var ref = new Firebase("https://dinosaur-facts.firebaseio.com/dinosaurs");
            //ref.orderByChild("height").equalTo(25).on("child_added", function(snapshot) {
            //  console.log(snapshot.key());
            //});            

            ManualResetEvent done = new ManualResetEvent(false);

            using (FirebaseApp app = new FirebaseApp(new Uri("https://dinosaur-facts.firebaseio.com/")))
            {
                var scoresRef = app.Child("dinosaurs")
                                   .OrderByChild("height")
                                   .EqualTo(25)
                                   .On("child_added",
                    (snapshot, child, context) =>
                    {
                        Console.WriteLine(snapshot.Key);
                        done.Set();
                    });

                done.WaitOne(TimeSpan.FromSeconds(15));
            }


        }

    }
}
