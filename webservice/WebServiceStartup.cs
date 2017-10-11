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
          + "    POST /upload Initiate an upload.\n"
          + "                 The client posts the length of the file it will upload.\n"
          + "                 On success, the server returns the upload ID to use with the WebSocket messages.\n"
          + "\n"
          + "A clients post a file name to /upload and receive an ID.\n"
          + "The client next opens a web socket to ws://<srvr>:54321/upload/<id>, and copies a files bytes to the socket.";

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

            uploads        = new Dictionary<int, string>();
            nextUploadID   = 1;
            idLock         = new object();
            uploadTokenRng = new Random();

            app
                .UseDeveloperExceptionPage()
                .UseRouter(BuildRoutes(app))
                .UseWebSockets()
                .Use(async (context, next) => 
                {
                    // If server does not know what else to do, try a Web Socket.
                    if (context.WebSockets.IsWebSocketRequest) 
                    {
                        // Assume that all WebSocket requests are for uploads.
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

        private string ExtractUploadToken(PathString requestPath)
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

            string tokenStr = ExtractUploadToken(httpCtx.Request.Path);
            log.LogInformation($"Upload: token {tokenStr} extracted from path.");

            int token = int.Parse(tokenStr);
            string uploadName;
            lock (uploads)
            {
                if ( ! uploads.ContainsKey(token) )
                {
                    log.LogWarning($"Upload: bad token value {tokenStr}");
                    return;
                }
                uploadName = uploads[token];
                uploads.Remove(token);
            }

            int totalBytes = 0;
            byte[] buffer = new byte[10240];

            using (FileStream fs = new FileStream(uploadName, FileMode.Create))
            {
                log.LogInformation($"Writing to {fs.Name}");

                for ( ; ; )
                {
                    try
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
                    catch (Exception ex)
                    {
                        log.LogWarning($"Upload error: {ex.Message}");
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

            int uploadNumber;
            int token;
            lock(idLock) 
            {
                token        = uploadTokenRng.Next();
                uploadNumber = this.nextUploadID++;    
            }

            lock(uploads)
            {
                uploads[token] = $"{uploadName}.{uploadNumber}";
            }

            log.LogInformation($"StartUpload: name '{uploadName}', id {uploadNumber}, token {token}");

            return httpCtx.Response.WriteAsync($"{token}");
        }


        #endregion


        private ILogger                 log;
        private Dictionary<int, string> uploads;
        private int                     nextUploadID;
        private object                  idLock;
        private Random                  uploadTokenRng;

        #region Routing

        /*
         * Set up URL handling for the Controller API.
         * URLs are listed above, in the initial file header.
         */
        private IRouter BuildRoutes(IApplicationBuilder app)
        {
            RouteBuilder routeBuilder = new RouteBuilder(app, new RouteHandler(null));

            routeBuilder.MapGet("",        ShowServiceDescription);
            routeBuilder.MapGet("index",   ShowServiceDescription);

            routeBuilder.MapPost("upload", StartUpload);

            return routeBuilder.Build();
        }

        #endregion

    }
}
