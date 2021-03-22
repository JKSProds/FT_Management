using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FT_Management.Models;
using System.Globalization;
using Quartz.Spi;
using Quartz;
using Quartz.Impl;
using System.Net.Sockets;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace FT_Management
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddMvc();

            //// Add Quartz services
            //services.AddSingleton<IJobFactory, SingletonJobFactory>();
            //services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

            //// Add our job
            //services.AddSingleton<HelloWorldJob>();
            //services.AddSingleton(new JobSchedule(
            //    jobType: typeof(HelloWorldJob),
            //    cronExpression: "00 00 23 * * ?")); 
            
            //services.AddHostedService<QuartzHostedService>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(cookieOptions =>
            {
                cookieOptions.LoginPath = "/Utilizadores/Login";
            });


            services.Add(new ServiceDescriptor(typeof(FT_ManagementContext), new FT_ManagementContext(Configuration.GetConnectionString("DefaultConnection"), Configuration.GetSection("Variaveis").GetSection("PrintLogo").Value)));
            services.Add(new ServiceDescriptor(typeof(TrelloConector), new TrelloConector(Configuration.GetSection("Trello").GetSection("API_KEY").Value, Configuration.GetSection("Trello").GetSection("TOKEN").Value)));
            services.Add(new ServiceDescriptor(typeof(PHCContext), new PHCContext(Configuration.GetConnectionString("PHCConnection"), Configuration.GetConnectionString("DefaultConnection"))));

            //var passwordHasher = new PasswordHasher<string>();
            //Console.WriteLine(passwordHasher.HashPassword(null, "Food@001.jmonteiro"));

            Console.WriteLine("A iniciar app. (V." + System.Reflection.Assembly.GetEntryAssembly().GetName().Version + ")");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            System.Globalization.CultureInfo customCulture = new CultureInfo("pt-PT");
            customCulture.NumberFormat.NumberDecimalSeparator = ".";

            CultureInfo.DefaultThreadCurrentCulture = customCulture;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseExceptionHandler("/Home/Error");
                //// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
                app.UseDeveloperExceptionPage();

            }

            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
            
        }
    }
}
