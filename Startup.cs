using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace wsfed_issue {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            // Add framework services.
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddDataProtection();
            services.Configure<ForwardedHeadersOptions>(options => {
                var addresses = Dns.GetHostAddresses("web");
                foreach (var address in addresses)
                    options.KnownProxies.Add(address);
            });

            services.AddAuthentication(
                    auth => {
                        auth.DefaultScheme = CookieAuthenticationDefaults
                            .AuthenticationScheme;
                        auth.DefaultChallengeScheme = WsFederationDefaults
                            .AuthenticationScheme;

                    })
                .AddCookie()
                .AddWsFederation(
                    WsFederationDefaults.AuthenticationScheme,
                    "Org Auth",
                    options => {
                        // MetadataAddress represents the Active Directory instance used to authenticate users.
                        options.MetadataAddress =
                            Configuration["WsAuthMetadataAddress"];
                        // Wtrealm is the app's identifier in the Active Directory instance.
                        // For ADFS, use the relying party's identifier, its WS-Federation Passive protocol URL:
                        options.Wtrealm = Configuration["WsAuthrealm"];
                    });

            // Simple example with dependency injection for a data provider.
            services
                .AddSingleton<Providers.IWeatherProvider,
                    Providers.WeatherProviderFake>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();

                // Webpack initialization with hot-reload.
                app.UseWebpackDevMiddleware(
                    new WebpackDevMiddlewareOptions {
                        HotModuleReplacement = true,
                    });
            } else {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseAuthentication();

            app.UseMvc(
                routes => {
                    routes.MapRoute(
                        name: "default",
                        template: "{controller=Home}/{action=Index}/{id?}");

                    routes.MapSpaFallbackRoute(
                        name: "spa-fallback",
                        defaults: new {controller = "Home", action = "Index"});
                });

            app.Use(Middleware);
        }

        private async Task Middleware(HttpContext context, Func<Task> next) {
            context.Response.ContentType = "text/plain";

            // Request method, scheme, and path
            await context.Response.WriteAsync(
                $"Request Method: {context.Request.Method}{Environment.NewLine}");
            await context.Response.WriteAsync(
                $"Request Scheme: {context.Request.Scheme}{Environment.NewLine}");
            await context.Response.WriteAsync(
                $"Request Path: {context.Request.Path}{Environment.NewLine}");

            // Headers
            await context.Response.WriteAsync($"Request Headers:{Environment.NewLine}");

            foreach (var header in context.Request.Headers)
                await context.Response.WriteAsync($"{header.Key}: " + $"{header.Value}{Environment.NewLine}");

            await context.Response.WriteAsync(Environment.NewLine);

            // Connection: RemoteIp
            await context.Response.WriteAsync(
                $"Request RemoteIp: {context.Connection.RemoteIpAddress}");
            await next();
        }
    }
}
