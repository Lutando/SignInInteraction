using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using Identity.Mvc.Constants;
using Identity.Sts.UI;
using Identity.Sts.UI.Login;

namespace Identity.Sts
{
    public class Startup
    {
        private IConfigurationRoot Configuration { get; set; }
        private IHostingEnvironment HostingEnvironment { get; set; }
        private X509Certificate2 Certificate { get; set; }

        public Startup(IHostingEnvironment env)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("config.json", true)
                .AddJsonFile($"config.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables()
                .Build();

            HostingEnvironment = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            Certificate = new X509Certificate2(Path.Combine(HostingEnvironment.ContentRootPath, "idsrv3test.pfx"), "idsrv3test");

            var builder = services.AddIdentityServer(options =>
            {
                options.UserInteractionOptions.LoginUrl = RoutePaths.LoginUrl;
                options.UserInteractionOptions.LogoutUrl = RoutePaths.LogoutUrl;
                options.UserInteractionOptions.ConsentUrl = RoutePaths.ConsentUrl;
                options.UserInteractionOptions.ErrorUrl = RoutePaths.ErrorUrl;
            });

            builder.SetSigningCredential(Certificate);
            builder.AddInMemoryClients(Config.GetClients());
            builder.AddInMemoryScopes(Config.GetScopes());
            builder.AddInMemoryUsers(Config.GetUsers());

            services
                .AddMvc()
                .AddRazorOptions(razor =>
                {
                    razor.ViewLocationExpanders.Add(new CustomViewLocationExpander());
                });
            services.AddTransient<LoginService>();

        }
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Trace);
            app.UseCors(b =>
            {
                b.AllowAnyHeader();
                b.AllowAnyMethod();
                b.AllowAnyOrigin();
                b.AllowCredentials();
            });
            app.UseIdentityServer();

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}
