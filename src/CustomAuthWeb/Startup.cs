using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using CustomAuthWeb.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using CustomAuthWeb.Services;
using CustomAuthWeb.Filters;

namespace CustomAuthWeb {
    public class Startup {
        public IConfigurationRoot Configuration { get; }

        public Startup(IHostingEnvironment env) {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
            if (env.IsDevelopment()) {
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services) {
            services.AddOptions();
            services.Configure<AppSecrets>(Configuration.GetSection("AppSecrets"));
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));
            services.AddSingleton<DbSeeder>();
            services.AddSingleton<AssetFileHash>();
            services.AddTransient<SimpleEncryptor>();
            services.AddTransient<SimpleHasher>();
            services.AddScoped<AuthenticationFilter>();

            // Using dependency injection here requires initializing the provider.
            // http://stackoverflow.com/questions/31863981/how-to-resolve-instance-inside-configureservices-in-asp-net-core
            var serviceProvider = services.BuildServiceProvider();
            var encryptorService = serviceProvider.GetService<SimpleEncryptor>();
            var dbContextService = serviceProvider.GetService<ApplicationDbContext>();


            services.AddMvc(config => {
                config.Filters.Add(new AuthenticationFilter(dbContextService, encryptorService));
            });
            // Must come after AuthenticationFilter
            services.AddScoped<AuthorizationFilter>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, DbSeeder seeder) {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }

            if (env.IsProduction()) {
                loggerFactory.AddFile("logs/CustomAuth.log");
                app.UseForwardedHeaders(new ForwardedHeadersOptions {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                });
            }

            app.UseStaticFiles();

            app.UseMvc(routes => {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            
            // seeder.SeedAsync().Wait();
        }
    }
}
