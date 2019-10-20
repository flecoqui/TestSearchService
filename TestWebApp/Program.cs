using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using System.ServiceProcess;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace TestWebApp
{
    public static class WebHostServiceExtensions
    {
        public static void RunAsCustomService(this IWebHost host)
        {
            var webHostService = new CustomWebHostService(host);
            ServiceBase.Run(webHostService);
        }
    }
    [DesignerCategory("Code")]
    internal class CustomWebHostService : WebHostService
    {
        private ILogger _logger;

        public CustomWebHostService(IWebHost host) : base(host)
        {
            _logger = host.Services
                .GetRequiredService<ILogger<CustomWebHostService>>();
        }

        protected override void OnStarting(string[] args)
        {
            _logger.LogInformation("OnStarting method called.");
            base.OnStarting(args);
        }

        protected override void OnStarted()
        {
            _logger.LogInformation("OnStarted method called.");
            base.OnStarted();
        }

        protected override void OnStopping()
        {
            _logger.LogInformation("OnStopping method called.");
            base.OnStopping();
        }
    }

    public partial class Program
    {
        static Int32 Version = ASVersion.SetVersion(0x01, 0x00, 0x00, 0x01);
        static Options ParseCommandLine(string[] args)
        {
            Options opt = Options.InitializeOptions(args);
            if (opt == null)
            {
                Options o = new Options();
                return null;
            }
            return opt;
        }
        public static bool InDocker { get { return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"; } }
        public static void Main(string[] args)
        {
            Options opt = Options.InitializeOptions(args);
            if (opt == null)
            {
                Options o = new Options();
                o.LogError("TestWebApp: Internal Error");
                return;
            }
            //opt.LogInformation("TestWebApp: Starting");
            string errorMessage = opt.GetErrorMessage();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                opt.LogError(errorMessage);
                opt.LogInformation(opt.GetInformationMessage(Version));
                return;
            }
            if (opt.ServiceMode == true)
            {
                opt.LogInformation("TestWebApp: Running as a service");
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                Directory.SetCurrentDirectory(pathToContentRoot);

                IWebHost host = null;
                var builder = CreateWebHostBuilder(args);
                if ((opt.UrlList != null) && (opt.UrlList.Count > 0))
                    host = builder.UseUrls(opt.UrlList.ToArray()).Build();
                else
                    host = builder.Build();
                opt.LogInformation("TestWebApp: Running as a service - RunAsCustomService");
                host.RunAsCustomService();
                //opt.LogInformation("TestWebApp: Running as a service - RunAsCustomService End");
                
            }
            else
            {
                if (opt.TestWebAppAction == Options.Action.Install)
                {
                    opt.LogInformation("Installing TestWebApp Service");
                    if (InstallService(opt) == true)
                        opt.LogInformation("Installing TestWebApp Service done");
                    return;
                }
                if (opt.TestWebAppAction == Options.Action.Uninstall)
                {
                    opt.LogInformation("Uninstalling TestWebApp Service");
                    if (UninstallService(opt) == true)
                        opt.LogInformation("Uninstalling TestWebApp Service done");
                    return;
                }
                if (opt.TestWebAppAction == Options.Action.Stop)
                {
                    opt.LogInformation("Stopping TestWebApp Service");
                    if (StopService(opt) == true)
                        opt.LogInformation("Stopping TestWebApp Service done");
                    return;
                }
                if (opt.TestWebAppAction == Options.Action.Start)
                {
                    opt.LogInformation("Starting TestWebApp Service");
                    if (StartService(opt) == true)
                        opt.LogInformation("Starting TestWebApp Service done");
                    return;
                }
                if (opt.TestWebAppAction == Options.Action.Help)
                {
                    opt.LogInformation(opt.GetInformationMessage(Version));
                    return;
                }

            }

            bool IsWindows = System.Runtime.InteropServices.RuntimeInformation
                   .IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
            if (IsWindows)
            {
                IWebHost host = null;
                var builder = CreateWebHostBuilder(args);
                if((opt.UrlList!=null)&&(opt.UrlList.Count>0))
                    host = builder.UseUrls(opt.UrlList.ToArray()).Build();
                else
                    host = builder.Build();
                host.Run();
            }
            else
            {
                IWebHost host = null;
                var builder = WebHost.CreateDefaultBuilder(args)
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        // Configure the app here.
                    })
                    .UseStartup<Startup>(); 
                if ((opt.UrlList != null) && (opt.UrlList.Count > 0))
                    host = builder.UseUrls(opt.UrlList.ToArray()).Build();
                else
                    host = builder.Build();
                host.Run();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                    .ConfigureLogging((hostingContext, logging) =>
                    {
                        logging.AddEventLog();
                    })
                    .ConfigureAppConfiguration((context, config) =>
                    {
                        // Configure the app here.
                    })
                .UseStartup<Startup>();

    }
}
