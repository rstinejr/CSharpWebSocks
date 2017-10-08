using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;


namespace waltonstine.demo.csharp.websockets.uploadservice
{
    /*
     * Startup claass used by .NET Core Web Service. Defines the "Routes", which is ASP-speak for the local part
     * of URI paths.
     * 
     * The ConfigureServices method enables logging and "routing" - URL handling.
     * 
     * The Configure method is used to define "routes" and associate URLs with handlers.
     */
    class WebServiceStartup
    {
        private static string ServiceDescription =
            "Demo of WebSockets. URLs:\n"
          + "    GET  /       Show this message\n"
          + "    GET  /hello  Display 'Hello, World!\n"
          + "    POST /upload Initiate an upload.\n" 
          + "                 The client posts the length of the file it will upload.\n"
          + "                 On success, the server returns the upload ID to use with the WebSocket messages.\n"
          + "\n"
          + "All web socket messages are presumed to be for an active upload.\n"
          + "Web socket upload messages are BSON messages in the following format:\n"
          + "    {upload_id:<id>,\n"
          + "     chunk_number:<seqno>,\n"
          + "     data:binary};\n";

        #region .NET Callbacks

        /*
         * ConfigureServices: called by the .NET Core web framework.
         * 
         * Denendency injection is used to add functionality to the web server.
         * "routing" is used to associate local URI paths with handler routines. 
         */
        public void ConfigureServices(IServiceCollection services) {
            services
                .AddRouting()
                .AddLogging();
        }

        /*
         * Configure: called by the .NET Core web framework.
         *
         * In .NET Core, the Configure method is primarily used to create routes and
         * associate them with handlers.
         * 
         * DEVLAN also uses this method for setting up the Controller.
         */
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory) {

            loggerFactory.AddConsole();

            log = loggerFactory
                    .CreateLogger("CSharpWebSock");

            nextUploadID = 1;
            idLock   = new object();

            app
                .UseDeveloperExceptionPage()   //.UseExceptionHandler("/error");
                .UseRouter(BuildRoutes(app))
                .UseWebSockets()
                .Use(async (context, next) => 
                {
                    if (context.WebSockets.IsWebSocketRequest) 
                    {
                        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await Upload(context, webSocket);
                    } 
                    else 
                    {
                        await next();
                    }
                });
        }

        #endregion

        #region Route Handlers

        ////////////////////////////////////////////////////////////////////////////////////
        /// Methods that respond to HTTP Requests
        ////////////////////////////////////////////////////////////////////////////////////

        private string ExtractUploadID(PathString requestPath)
        {
            string wrkPath = requestPath.ToString();
            int lastInx = wrkPath.LastIndexOf('/');
            return wrkPath.Substring(lastInx + 1);
        }

        /*
         * Upload: handler server side of WebSocket.
         * All WebSocket requests routed here.
         */
        private async Task Upload(HttpContext httpCtx, WebSocket sock)
        {
            log.LogInformation($"Upload method called: WebSocket request, request path is {httpCtx.Request.Path}");

            string id = ExtractUploadID(httpCtx.Request.Path);
            log.LogInformation($"Upload: ID {id} extracted from path.");
            int totalBytes = 0;
            byte[] buffer = new byte[10240];

            using (FileStream fs = new FileStream($"upload.{id}", FileMode.Create))
            {
                log.LogInformation($"Writing to {fs.Name}");

                for ( ; ; )
                {
                    WebSocketReceiveResult result =
                        await sock.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    log.LogDebug($"Received {result.Count} bytes from client.");
                    fs.Write(buffer, 0, result.Count);
                    totalBytes += result.Count;
                    if (result.EndOfMessage || result.CloseStatus != null)
                    {
                        break;
                    }
                }
                fs.Flush();
                fs.Close();
            }

            return;
        }


        private Task ShowServiceDescription(HttpContext httpCtx) 
        {
            return httpCtx.Response.WriteAsync(ServiceDescription);
        }

        /*
         * Initiate an upload. Return an upload ID.
         */
        private Task StartUpload(HttpContext httpCtx)
        {
            StreamReader rdr = new StreamReader(httpCtx.Request.Body);
            string uploadName = rdr.ReadToEnd();

            int returnedID;

            lock(idLock) 
            {
                returnedID = this.nextUploadID++;    
            }

            log.LogInformation($"StartUpload: name '{uploadName}', id {returnedID}");

            return httpCtx.Response.WriteAsync($"{returnedID}");
        }

        /*
         * Handle "GET /hello"
         */
        private Task GotHello(HttpContext httpCtx)
        {
            return httpCtx.Response.WriteAsync("Hello, World!");
        }

        /*
         * Dummy request handler to use until real guts can be put together.
         * It echos the request, to indicate that the HTTP part of the server is hooked up.
         */
        private Task EchoRequest(HttpContext httpCtx)
        {
            if (httpCtx.Request.Method == "POST")
            {
                StreamReader rdr = new StreamReader(httpCtx.Request.Body);
                string body = rdr.ReadToEnd();
                log.LogDebug($"Body of POST: {body}");
            }

            return httpCtx.Response.WriteAsync(httpCtx.Request.Path.ToString());
        }

        #endregion


        private ILogger                 log;
        private Dictionary<int, string> uploads;
        private int                     nextUploadID;
        private object                  idLock;

        #region Routing

        /*
         * Set up URL handling for the Controller API.
         * URLs are listed above, in the initial file header.
         */
        private IRouter BuildRoutes(IApplicationBuilder app)
        {
            RouteBuilder routeBuilder = new RouteBuilder(app, new RouteHandler(null));

            routeBuilder.MapGet("",        ShowServiceDescription);
            routeBuilder.MapGet("hello",   GotHello);
            routeBuilder.MapPost("upload", StartUpload);

            return routeBuilder.Build();
        }

        #endregion

    }
}
