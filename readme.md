FirebaseSharp
==============

A Firebase API for .NET.

# Fun Facts

- This is a .NET Portable Library.
- It currently only consumes and delivers JSON - maybe I'll add type converters to the API tomorrow.
- It has synch and async (TPL) versions of all methods.
- It probably sucks.

# Usage

## Create the Firebase object

    Firebase fb = new Firebase(new Uri("https://dazzling-fire-1575.firebaseio.com/"));

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


