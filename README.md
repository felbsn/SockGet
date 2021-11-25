# SockGet

Simple socket wrapper

- receive & send c# objects,streams
- async send, request and response mechanisms
- simple authentication
- server side client handling


### Usage 

```csharp



client.Send("string head", "and object body");
client.Send("body can also be a", someCsharpObject);
client.Send("or a stream", new FileStream("test.data", FileMode.Open));

// blocks until response
var response = client.Request("request waits", "for response");

```


### Client
```csharp

client = new SgClient();

client.DataReceived += (s,e) => {
  if(e.Data.Head == "i sent a head message so")
  {
    var IknowMyData data = e.Data.As<IknowMyData>();
    //todo: handle this...
  }
    //also you can return your response, check if you dont know... e.IsResponseRequired
    e.Response = Response.From("that is just fine", ("tuples.." ,data ));
};

client.AuthToken = "an optional string...";
client.Connect("127.0.0.1" ,9333);

```

### Server
```csharp

var server = new SgServer();

// if you want to cancel incoming connections, default is accept by the way
server.ClientAuthRequested += (s, e) => {
    string token = e.Token;
    e.Reject = false; // reject it if you dont like its token or Tags...
}

server.ClientConnected += (s, e) =>
{
  var client = e.Client;
  client.DataReceived += (ss, ee) => { /** handle incoming data **/ } 
}

// start the server
server.Serve(9333);
```




## Heartbeat

SgServer has a built-in heartbeat feature, which you can enable like this

```csharp

server.UseHeartbeat = true; // false by default

// you can adjust these values
server.HeartbeatInterval = 5_000; 
server.HeartbeatTimeout = 5_000; 

```


## Serializer

You can use your own serializer if you wish, set default serializer before instancing sockget.

Note: Some in-built features (Tag sync, heartbeat) needs serializer, i mean broken serializer is a problem. By default library uses newtonsoft. 

```csharp
SgSocket.Serializer = new MySerializer(); // any serializer that implements ISerializer
```




