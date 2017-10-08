# CSharpWebSocks
Minimal Web Server, CLI clients, that provide simple file upload by using Web Sockets.

This project illustrates the use of Web Sockets in .NET Core. Note that Web Sockets are not natively supported in Windows 7.

# API

The API for the upload server is:

* GET / : display info about the provided service and supported URIs.    
* POST /upload : Request initiation of an upload. The client posts a file URL; the server responds with an upload ID.
* <Web Socket> : Use BSON to serialize upload. A file is uploaded in fixed chunks; the last chunk will probably be truncated but all others will be the constant chunk size. Output is serialized using BSON. The uploaded packet includes the upload ID, the chunk numnber, and a byte array of file content.

# Development Environment

Develooped on 64-bit Windows 10, using ```netcorapp2.0```.

You may dowonload the .NET Core runtime, SDK, and Visual Studio 2017 IDE from http://dot.net
