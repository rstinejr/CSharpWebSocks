# CSharpWebSocks
Minimal Web Server, CLI clients, that provide simple file upload by using Web Sockets.

This project illustrates the use of Web Sockets in .NET Core. 

**WARNING!** Because there is no native support for websockets in Windows 7, .NET Core cannot create web sockets on that OS. You get a runtime error if you try.

# API

The API for the upload server is:

* GET / : display info about the provided service and supported URIs.    
* POST /upload : Request initiation of an upload. The client posts the name of the upload; the server responds with an upload ID.
* <Web Socket> /upload/<upload ID> : Send a byte stream; let TCP take care of segmenting and reassemby. The target upload is identified by the uploadId in the URL.


# Bonus

In addition to demonstrating the use of Web Sockets, this project shows some handy 
techniques and info: 

* Post an HTTP request from a console app, and get the response.
* On the server side, read the body of a POST with StreamReader.

# Build

1. ```git clone git@github.com:rstinejr/CSharpWebSocks.git```
2. ```cd CSharpWebSocks```
3. ```pushd webservice```
4. ```dotnet restore```
5. ```dotnet build```
6. ```popd```
7. ```pushd webclientCli```
8. ```dotnet restore```
9. ```dotnet build```
10. ```popd```

# Run

Start the server. In one console window,

1. ```pushd webservice```
2. ```dotnet run```

In another console windows;

1. ```pushd webclientCli```
2. ```dotnet run localhost data.xml```

This will upload test file data.xml to file webservice\data.xml.1, assuming this is the first upload handled 
by the server process. Note that the contents of the source and uploaded files
are identical.

# API

The web server exposes three APIs:

* ```http: /``` - Display text message that descibes the service.
* ```http: /upload``` - The client pushes an upload name, as text/plain to the server. The server response with a single use upload token.
* ```ws: /upload/<token>```` - The client opens a web socket and includes the upload token in the URI. If the token matches a granted but unused upload request, then it
writes the byte stream from the web socket to a file.


At present, the server uses Dictionary<int, string> as a cache for pending upload requests.
My son, Robert W. Stine, suggested that it would be more secure to keep the upload token 
in a web session. Great idea! But in .NET Core, using a Web session without also using the whole MVC framework appears to be a task worthy of its own project.  For now, 
I'll use the Dicsionary.

# Development Environment

Developed on 64-bit Windows 10, using ```netcorapp2.0```.

Built on 64-bit Linux Mint 17.2 (based on Ubuntu 14.4), but in testing ran into this 
error: [WebSocket connection is closed without proper handshake](https://github.com/aspnet/AspNetCoreModule/issues/77).
Added an ugly hack - a Thread.Sleep() on the client - to handle .NET bug.
 

# Installing .NET Core

You can get the .NET Core runtime, SDK, and Visual Studio 2017 IDE from http://dot.net
