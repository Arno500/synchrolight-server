using DBModels;
using LightServer.Managers;
using LightServer.Pages;
using LightServer.Server;
using LightServer.Server.Hubs;
using Makaretu.Dns;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace LightServer
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        internal WebApplication host;

        public DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            // Prepare DB
            new BeatsheetContext().Database.MigrateAsync();

            // Initialize settings
            new SettingsManager();

            var builder = WebApplication.CreateBuilder();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: "AllowAnything",
                    policy =>
                    {
                        policy.AllowAnyOrigin().AllowAnyMethod();
                    });
            });

            builder.Services.AddSignalR();

            host = builder.Build();

            host.MapHub<BeaconHub>("/beacons");
            new WebSocket(host.Services.GetRequiredService(typeof(IHubContext<BeaconHub>)) as IHubContext<BeaconHub>);
            
            host.Urls.Add("http://*:64321");
            
            host.UseCors("AllowAnything");

            new Http().Configure(host);
            host.RunAsync();

            // Start ArtNet client
            new ArtNetServer();

            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            string address = host.Urls.First();
            int port = int.Parse(address.Split(':').Last());
            var service = new ServiceProfile("SynchroLight", "_synchrolight._tcp", (ushort)port);
            var sd = new ServiceDiscovery();
            service.AddProperty("proto", "signalr");
            sd.Advertise(service);
            m_window = new MainWindow();
            m_window.Activate();
        }

        private Window m_window;
    }
}
