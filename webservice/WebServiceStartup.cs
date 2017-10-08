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
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace waltonstine.demo.csharp.webservice
{
    /**
     * Startup claass used by .NET Core Web Service. Defines the "Routes", which is ASP-speak for the local part
     * of URI paths.
     * 
     * The ConfigureServices method enables logging and "routing" - URL handling.
     * 
     * The Configure method is used to define "routes" and associate URLs with handlers.
     */
    class WebServiceStartup
    {

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

            app
                .UseDeveloperExceptionPage()   //.UseExceptionHandler("/error");
                .UseRouter(BuildRoutes(app));
        }


        #endregion

        #region Route Handlers

        ////////////////////////////////////////////////////////////////////////////////////
        /// Methods that respond to HTTP Requests
        ////////////////////////////////////////////////////////////////////////////////////


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
        private Task EchoRequest(HttpContext httpCtx) {
            if (httpCtx.Request.Method == "POST") {
                StreamReader rdr = new StreamReader(httpCtx.Request.Body);
                string body = rdr.ReadToEnd();
                log.LogDebug($"Body of POST: {body}");
            }

            return httpCtx.Response.WriteAsync(httpCtx.Request.Path.ToString());
        }

        #endregion


        private ILogger            log;


        #region Routing

        /*
         * Set up URL handling for the Controller API.
         * URLs are listed above, in the initial file header.
         */
        private IRouter BuildRoutes(IApplicationBuilder app) {

            RouteBuilder routeBuilder = new RouteBuilder(app, new RouteHandler(null));

            routeBuilder.MapGet("",      GotHello);
            routeBuilder.MapGet("hello", GotHello);

            return routeBuilder.Build();
        }

        #endregion

    }
}
