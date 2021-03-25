using System;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Web {

    public class Startup {

        public static readonly HttpClient Client;

        static Startup() {
            Client = new HttpClient();
            Client.DefaultRequestHeaders.Add(HeaderNames.UserAgent, "WOMConnector/1.0");
        }

        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services) {
            services.AddRazorPages();
        }

        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            ILogger<Startup> logger
        ) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            // Fix incoming base path for hosting behind proxy
            string basePath = Environment.GetEnvironmentVariable("ASPNETCORE_BASEPATH");
            if(!string.IsNullOrWhiteSpace(basePath)) {
                logger.LogInformation("Configuring server to run under base path '{0}'", basePath);

                app.UsePathBase(new PathString(basePath));
                app.Use(async (context, next) => {
                    context.Request.PathBase = basePath;
                    await next.Invoke();
                });
            }

            // Enable forwarded headers within Docker local networks
            var forwardOptions = new ForwardedHeadersOptions {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto
            };
            forwardOptions.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("172.20.0.1"), 2));
            app.UseForwardedHeaders(forwardOptions);

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints.MapRazorPages();
            });
        }

    }

}
