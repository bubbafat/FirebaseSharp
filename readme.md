FirebaseSharp 2.0
==============

A (new) Firebase API for .NET.

This is the alpha FirebaseSharp 2.0 library - some things are broken, other are missing.  There are many happy accidents that seem to work.

# Usage

## Create the FirebaseApp object

```CSharp
// this is IDisposable ... please clean up when done!
FirebaseApp app = new FirebaseApp(new Uri("https://dinosaur-facts.firebaseio.com/") /*, <auth token> */);
```

## Subscribe to a location

```CSharp
var scoresRef = app.Child("scores");
```

## Perform a query

```CSharp
scoresRef.OrderByValue()
         .LimitToLast(3)
         .On("value", (snapshot, child, context) => {
  foreach (var data in snapshot.Children) {
    Console.WriteLine("The {0} dinosaur\'s score is {1}",
                        data.Key, data.Value<int>());
   }
});
```
    
