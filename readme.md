FirebaseSharp
==============

A Firebase API for .NET.

# Fun Facts

- This is a .NET Portable Library
- It has synch and async (TPL) versions of all methods
- It supports streaming gets and produces events for item added, updated, and removed
- Custom headers are not yet supported
- It throws on error HTTP status codes  (happy path is always success)
- It probably sucks

# Updates

- Auth is now supported.  Pass in your token to the Firebase constructor
- Events during streaming reads are now generated for add, update and remove

# Usage

## Create the Firebase object

    Firebase fb = new Firebase(new Uri("https://dazzling-fire-1575.firebaseio.com/"));

## Create the Firebase object with an auth token

    string rootUri = "https://dazzling-fire-1575.firebaseio.com/";
    string authToken = "YOUR FIREBASE AUTH TOKEN";
        
    Firebase fb = new Firebase(rootUri, authToken);

## Post Data

    string path = "/path";
    string data = "{{\"value\": \"Hello!\"}}";
        
    string id = fb.Post(path, data);
    
## Get Data

    string jsonData = gb.Get(path);
   
## Stream Data

    fb.GetStreaming("path/to/monitor", 
        added: (s, args) => AddedItem(args),
        changed: (s, args) => UpdatedItem(args),
        removed: (s, args) => RemovedItem(args));
                
                
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
    
## Delete Data

    fb.Delete(path);
   
## The Rest

It's pretty self-explanatory.


