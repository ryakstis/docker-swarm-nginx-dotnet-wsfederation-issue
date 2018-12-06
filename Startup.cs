using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.WsFederation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using wsfed_issue.Providers;

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
            services.Configure<ForwardedHeadersOptions>(
                ConfigureForwardedHeaders);

            AddAuthenticationTo(services);

            // Simple example with dependency injection for a data provider.
            services.AddSingleton<IWeatherProvider, WeatherProviderFake>();
        }

        private void ConfigureForwardedHeaders(ForwardedHeadersOptions options) {
            var address = IPAddress.Parse("::ffff:172.18.0.0");
            var network = new IPNetwork(address, 8);
            options.KnownNetworks.Add(network);
        }

        private void AddAuthenticationTo(IServiceCollection services) {
            var builder = services.AddAuthentication(ConfigureAuthentication);
            builder.AddCookie();
            builder.AddWsFederation(
                WsFederationDefaults.AuthenticationScheme,
                "Org Auth",
                ConfigureWsFederation);
        }

        private void ConfigureAuthentication(AuthenticationOptions auth) {
            auth.DefaultScheme =
                CookieAuthenticationDefaults.AuthenticationScheme;
            auth.DefaultChallengeScheme =
                WsFederationDefaults.AuthenticationScheme;
        }

        private void ConfigureWsFederation(WsFederationOptions options) {
            // MetadataAddress represents the Active Directory instance used to authenticate users.
            options.MetadataAddress = Configuration["WsAuthMetadataAddress"];

            // Wtrealm is the app's identifier in the Active Directory instance.
            // For ADFS, use the relying party's identifier, its WS-Federation Passive protocol URL:
            options.Wtrealm = Configuration["WsAuthrealm"];
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env) {
            if (env.IsDevelopment())
                ConfigureDevelopmentFor(app);
            else
                app.UseExceptionHandler("/Home/Error");

            app.UseStaticFiles();
            var forwardedOptions = new ForwardedHeadersOptions {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor
                    | ForwardedHeaders.XForwardedProto
            };
            app.UseForwardedHeaders(forwardedOptions);
            //app.Use(Middleware);
            app.UseAuthentication();

            app.UseMvc(ConfigureRoutes);
        }

        private void ConfigureDevelopmentFor(IApplicationBuilder app) {
            var webpackOptions = new WebpackDevMiddlewareOptions {
                HotModuleReplacement = true,
            };

            // Webpack initialization with hot-reload.
            app.UseWebpackDevMiddleware(webpackOptions);
            app.UseDeveloperExceptionPage();
        }

        private void ConfigureRoutes(IRouteBuilder routes) {
            routes.MapRoute(
                "default",
                "{controller=Home}/{action=Index}/{id?}");
            routes.MapSpaFallbackRoute(
                "spa-fallback",
                new {controller = "Home", action = "Index"});
        }

        private async Task Middleware(HttpContext context, Func<Task> next) {
            var newline = Environment.NewLine;
            var request = context.Request;
            var response = context.Response;
            response.ContentType = "text/plain";

            // Request method, scheme, and path
            await response.WriteAsync(
                $"Request Method: {request.Method}{newline}");
            await response.WriteAsync(
                $"Request Scheme: {request.Scheme}{newline}");
            await response.WriteAsync($"Request Path: {request.Path}{newline}");

            // Headers
            await response.WriteAsync($"Request Headers:{newline}");

            foreach (var header in request.Headers)
                await response.WriteAsync(
                    $"{header.Key}: {header.Value}{newline}");

            await response.WriteAsync(newline);

            // Connection: RemoteIp
            await response.WriteAsync(
                $"Request RemoteIp: {context.Connection.RemoteIpAddress}{newline}");

            await next();
        }
    }
}
