FirebaseSharp
==============

A Firebase API for .NET.

# Fun Facts

- This is a .NET Portable Library
- It currently only consumes and delivers JSON
- It has synch and async (TPL) versions of all methods
- It supports streaming gets and parses the event 
- Custom headers are not yet supported
- It throws on error HTTP status codes  (happy path is always success)
- It probably sucks

# Updates

- Auth is now supported.  Pass in your token to the Firebase constructor

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

    Response resp = fb.GetStreaming(path, response => {
       // see https://www.firebase.com/docs/rest-api.html
       // response.Event
       // response.Payload
    });
        
    // resp.Dispose() !
    
## Delete Data

    fb.Delete(path);
   
## The Rest

It's pretty self-explanatory.


