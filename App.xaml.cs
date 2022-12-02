using DBModels;
using LightServer.Managers;
using LightServer.Server;
using LightServer.Server.Hubs;
using Makaretu.Dns;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using static Haukcode.Rdm.Packets.Status.StatusMessage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LightServer
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        internal IHost host;
        internal static int port;

        public DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            // Prepare DB
            new BitsheetContext().Database.MigrateAsync();

            // Initialize settings
            new SettingsManager();

            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
            {
                services.AddCors(options =>
                {
                    options.AddPolicy(name: "AllowAnything",
                        policy =>
                        {
                            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                        });
                });
                services.AddSignalR();
            })
                .ConfigureWebHostDefaults(configure =>
            {
                configure.UseStartup<HttpStartup>().UseConfiguration(configuration);
            });

            host = builder.Build();

            new WebSocket(host.Services.GetRequiredService(typeof(IHubContext<BeaconHub>)) as IHubContext<BeaconHub>);

            host.RunAsync();

            var server = host.Services.GetService<IServer>();
            var addressFeature = server.Features.Get<IServerAddressesFeature>();

            var boundAddress = new Uri(addressFeature.Addresses.First());
            port = boundAddress.Port;

            // Start ArtNet client
            new ArtNetServer();

            // Start Bitsheet matching and reading system;
            new BitSheetManager();

            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            //string address = host.Urls.First();
            //int port = int.Parse(address.Split(':').Last());
            var service = new ServiceProfile("SynchroLight", "_synchrolight._tcp", (ushort)App.port);
            var sd = new ServiceDiscovery();
            service.AddProperty("proto", "signalr");
            sd.Advertise(service);
            m_window = new MainWindow();
            m_window.Activate();
        }

        private Window m_window;
    }
}
