using LightServer.Server;
using LightServer.Server.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace LightServer
{
    internal class HttpStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();
            app.UseRouting();

            app.UseCors("AllowAnything");

            app.UseEndpoints(endpoints => {
                new Http().Configure(endpoints);
                endpoints.MapHub<BeaconHub>("/beacons");
            });

            app.UseHttpsRedirection();

            app.UseFileServer(new FileServerOptions
                {
                    FileProvider = new PhysicalFileProvider(
                        Path.Combine(env.ContentRootPath, "Public")),
                    RequestPath = ""
                });
        }
    }
}
