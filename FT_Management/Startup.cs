using System;
using System.Web;
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
using Microsoft.AspNetCore.DataProtection;
using LettuceEncrypt;
using System.IO;

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

            services.Add(new ServiceDescriptor(typeof(FT_ManagementContext), new FT_ManagementContext(Configuration.GetConnectionString("DefaultConnection"), Configuration.GetSection("Variaveis").GetSection("PrintLogo").Value)));
            services.Add(new ServiceDescriptor(typeof(PHCContext), new PHCContext(Configuration.GetConnectionString("PHCConnection"), Configuration.GetConnectionString("DefaultConnection"))));

            // Add Quartz services
            services.AddSingleton<IJobFactory, SingletonJobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development")
            {
                if (FT_ManagementContext.ObterParam("EnvioEmailFerias", Configuration.GetConnectionString("DefaultConnection")) == "1")
                {
                    // Add our job
                    services.AddSingleton<CronJobFerias>();
                    services.AddSingleton(new JobSchedule(
                        jobType: typeof(CronJobFerias),
                        cronExpression: FT_ManagementContext.ObterParam("DataEnvioEmailFerias", Configuration.GetConnectionString("DefaultConnection"))));
                }

                if (FT_ManagementContext.ObterParam("EnvioEmailAgendamentoComercial", Configuration.GetConnectionString("DefaultConnection")) == "1")
                {
                    services.AddSingleton<CronJobAgendamentoCRM>();
                    services.AddSingleton(new JobSchedule(
                        jobType: typeof(CronJobAgendamentoCRM),
                        cronExpression: FT_ManagementContext.ObterParam("DataEnvioEmailAgendamentoComercial", Configuration.GetConnectionString("DefaultConnection"))));
                }


                if (FT_ManagementContext.ObterParam("EnvioEmailAniversario", Configuration.GetConnectionString("DefaultConnection")) == "1")
                {
                    services.AddSingleton<CronJobAniversario>();
                    services.AddSingleton(new JobSchedule(
                        jobType: typeof(CronJobAniversario),
                        cronExpression: FT_ManagementContext.ObterParam("DataEnvioEmailAniversario", Configuration.GetConnectionString("DefaultConnection"))));
                }

                if (FT_ManagementContext.ObterParam("SaidaAutomatica", Configuration.GetConnectionString("DefaultConnection")) == "1")
                {
                    services.AddSingleton<CronJobSaida>();
                    services.AddSingleton(new JobSchedule(
                        jobType: typeof(CronJobSaida),
                        cronExpression: FT_ManagementContext.ObterParam("DataSaidaAutomatica", Configuration.GetConnectionString("DefaultConnection"))));
                }
            }

            //COPIAR IMAGENS UTILIZADOR
            FicheirosContext.ObterImagensUtilizador();

            services.AddHostedService<QuartzHostedService>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(cookieOptions =>
            {
                cookieOptions.LoginPath = "/Utilizadores/Login";
                cookieOptions.AccessDeniedPath = "/Home/AcessoNegado";
            });

#if !DEBUG
            services.AddDataProtection().SetApplicationName("FT_Management").PersistKeysToFileSystem(new DirectoryInfo("/https/"));
                 services.AddLettuceEncrypt().PersistDataToDirectory(new DirectoryInfo("/https/"), "Password123");
#endif
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime appLifetime)
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
                app.UseExceptionHandler("/Home/Error");
                //app.UseHsts();
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

            appLifetime.ApplicationStarted.Register(() =>
            {
                Console.WriteLine("Iniciada a aplicação!");
            });

            appLifetime.ApplicationStopping.Register(() =>
            {
                Console.WriteLine("A aplicação está a parar...");
            });

            appLifetime.ApplicationStopped.Register(() =>
            {
                Console.WriteLine("A aplicação parou!");
            });

        }
    }
}
