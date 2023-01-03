using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FT_Management.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace FT_Management
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //Check for the Debugger is attached or not if attached then run the application in IIS or IISExpress
            var isService = false;

            //when the service start we need to pass the --service parameter while running the .exe
            if (Debugger.IsAttached == false && args.Contains("--service"))
            {
                isService = true;
            }

            if (isService)
            {
                //Get the Content Root Directory
                var pathToContentRoot = Directory.GetCurrentDirectory();

                //If the args has the console element then than make it in array.
                var webHostArgs = args.Where(arg => arg != "--console").ToArray();

                string portNo = "1997"; //Port

                var host = WebHost.CreateDefaultBuilder(webHostArgs)
                .UseContentRoot(pathToContentRoot)
                .UseStartup<Startup>()
                .UseUrls("http://*:" + portNo)
                .Build();

                host.RunAsService();
            }
            else
            {
                CreateHostBuilder(args).Build().Run();
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //webBuilder.UseSetting("https_port", "443");
                    webBuilder.UseStartup<Startup>();
                });
    }
}
