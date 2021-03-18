namespace Fanex.SessionStateProvider.RedisCore.Example
{
    using System;
    using Fanex.SessionStateProvider.AspNetCore.Redis;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
#pragma warning disable S2325 // Methods and properties that don't access instance data should be static

        public void ConfigureServices(IServiceCollection services)
        {
            const int sessionTimeoutInSeconds = 120;

            services.AddRedisSession(
                redisCacheOptions =>
                {
                    redisCacheOptions.ApplicationName = "Fanex.SessionStateProvider.RedisCore.Example";
                    redisCacheOptions.ConnectionString = "ha.nexdev.net:6379";
                },
                sessionOptions =>
                {
                    sessionOptions.IdleTimeout = TimeSpan.FromSeconds(sessionTimeoutInSeconds); // session timeout
                });
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSession();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}