# CSharpWebSocks
Minimal Web Server, CLI clients, that provide simple file upload by using Web Sockets.

This project illustrates the use of Web Sockets in .NET Core. Note that Web Sockets are not natively supported in Windows 7.

# API

The API for the upload server is:

* GET / : display info about the provided service and supported URIs.    
* POST /upload : Request initiation of an upload. The client posts the length of the file it will upload; the server responds with an upload ID.
* <Web Socket> : Use BSON to serialize upload. A file is uploaded in fixed chunks; the last chunk will probably be truncated but all others will be the constant chunk size. Output is serialized using BSON. The uploaded packet includes the upload ID, the chunk numnber, and a byte array of file content.

# Bonus

In addition to demonstrating the use of Web Sockets, this project shows some handy 
techniques and info: 

* Workable directory layout for unit test and projects
* Read the body of a POST with StreamReader.
* Convert a file URL to its local OS file path using new Uri(fileURL).LocalPath.
* Use of BSON for serialization.

# Development Environment

Develooped on 64-bit Windows 10, using ```netcorapp2.0```.

You can get the .NET Core runtime, SDK, and Visual Studio 2017 IDE from http://dot.net
