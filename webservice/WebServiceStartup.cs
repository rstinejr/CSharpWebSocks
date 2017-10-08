using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace waltonstine.dotnetdemo.CSharpWebSocks.webservice
{
    public class WebServiceStartup
    {
        public WebServiceStartup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddRouting()
                .AddLogging();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            app
                .UseDeveloperExceptionPage()
                .UseRouter(BuildRoutes(app));
        }

        private Task GotHello(HttpContext httpCtx)
        {
            return httpCtx.Response.WriteAsync("hello");
        }

        private IRouter BuildRoutes(IApplicationBuilder app)
        {
            RouteBuilder routeBuilder = new RouteBuilder(app, new RouteHandler(null));

            routeBuilder.MapGet("hello", GotHello);

            return routeBuilder.Build();
        }

    }
}
