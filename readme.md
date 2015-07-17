FirebaseSharp 2.0
==============

A (new) Firebase API for .NET.

This is work-in-progress and is not ready yet.  Most things don't work.  Those that do are probably happy accidents.

# Usage

## Create the FirebaseApp object

```CSharp
FirebaseApp app = new FirebaseApp(new Uri("https://dinosaur-facts.firebaseio.com/"));
```

## Subscribe to a location

```CSharp
var scoresRef = app.Child("scores");
```

## Perform a query

```CSharp
scoresRef.OrderByValue<int>().LimitToLast(3).On("value", (snapshot, child, context) => {
  foreach (var data in snapshot.Children) {
    Console.WriteLine("The {0} dinosaur\'s score is {1}",
                        data.Key, data.Value<int>());
   }
});
```
    


