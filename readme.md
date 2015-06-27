FirebaseSharp
==============

A Firebase API for .NET.

# Fun Facts

- This is a .NET Portable Library
- It has only async (TPL) methods (the sync methods have been removed)
- It supports streaming gets and produces events for item added, updated, and removed, auth revoked and closed
- Custom headers are not yet supported
- It throws on error HTTP status codes  (happy path is always success)
- It probably sucks

# Updates

- Auth is now supported.  Pass in your token to the Firebase constructor
- Events during streaming reads are now generated for add, update and remove
- Improved support for event: patch
- All sync methods removed

# Usage

## Create the Firebase object

```CSharp
Firebase fb = new Firebase(new Uri("https://dazzling-fire-1575.firebaseio.com/"));
```

## Create the Firebase object with an auth token

```CSharp
string rootUri = "https://dazzling-fire-1575.firebaseio.com/";
string authToken = "YOUR FIREBASE AUTH TOKEN";
        
Firebase fb = new Firebase(rootUri, authToken);
```

## Post Data

```CSharp
string path = "/path";
string data = "{{\"value\": \"Hello!\"}}";
        
string id = await fb.PostAsync(path, data);
```

## Get Data

```CSharp
string jsonData = await gb.GetAsync(path);
```   

## Stream Data
```CSharp
using(var response = fb.GetStreaming("path/to/monitor", 
        added: (s, args) => AddedItem(args),
        changed: (s, args) => UpdatedItem(args),
        removed: (s, args) => RemovedItem(args),
        cancellationToken /* optional */))
{
    // use it
}
            
            
private void AddedItem(ValueAddedEventArgs args)
{
    // process addition
}

private void RemovedItem(ValueRemovedEventArgs args)
{
    // process removal
}

private void UpdatedItem(ValueChangedEventArgs args)
{
    // process update
}
```

## Delete Data

```CSharp
fb.Delete(path);
```

## The Rest

It's pretty self-explanatory ... though I find when I don't look at the code for a few months it gets fuzzy.  So I'm trying to improve that a little with some refactoring.



