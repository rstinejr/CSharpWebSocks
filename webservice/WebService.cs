using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;

namespace waltonstine.demo.csharp.webservice
{
  
    public class WebService
    {

        private static IConfigurationRoot conf;

        public static IConfigurationRoot Conf {
            get { return conf; }
        }

        public static void Main(string[] args)
        {

            Console.WriteLine($"Start web server, current dir is {Directory.GetCurrentDirectory()}");

            IConfigurationBuilder confBldr =
               new ConfigurationBuilder();

            conf = confBldr.Build();

            IWebHost host =
                new WebHostBuilder()
                 //.ConfigureServices(services => { services.AddSingleton<IEvidenceUploadManager, EvidenceUploadManager>(); })
                 .UseKestrel()
                 .UseContentRoot(Directory.GetCurrentDirectory())
                 .UseUrls("http://*:54321")
                 .UseStartup<WebServiceStartup>()
                 .Build();

            host.Run();

            // No return; host.Run() kicks off the Kestrel web server.
            // Exit with ^C.
        }
    }
}
