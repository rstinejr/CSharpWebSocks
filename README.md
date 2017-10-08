# CSharpWebSocks
Minimal Web Server, CLI clients, that provide simple file upload by using Web Sockets.

This project illustrates the use of Web Sockets in .NET Core. Note that Web Sockets are not natively supported in Windows 7.

# API

The API for the upload server is:

* GET / : display info about the provided service and supported URIs.    
* POST /upload : Request initiation of an upload. The client posts the name of the upload; the server responds with an upload ID.
* <Web Socket> : Send a byte stream; let TCP take care of segmenting and reassemby.


# Bonus

In addition to demonstrating the use of Web Sockets, this project shows some handy 
techniques and info: 

* Workable directory layout for unit test and projects
* Read the body of a POST with StreamReader.

# Development Environment

Develooped on 64-bit Windows 10, using ```netcorapp2.0```.

You can get the .NET Core runtime, SDK, and Visual Studio 2017 IDE from http://dot.net
