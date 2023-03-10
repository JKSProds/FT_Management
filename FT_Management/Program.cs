using Microsoft.Extensions.Configuration;
using System.Globalization;
using AspNetCoreRateLimit;

namespace FT_Management
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Serviços
            builder.Services.AddControllersWithViews();
            builder.Services.AddMvc();

            builder.Services.Add(new ServiceDescriptor(typeof(FT_ManagementContext), new FT_ManagementContext(builder.Configuration.GetConnectionString("DefaultConnection"))));
            builder.Services.Add(new ServiceDescriptor(typeof(PHCContext), new PHCContext(builder.Configuration.GetConnectionString("PHCConnection"), builder.Configuration.GetConnectionString("DefaultConnection"))));

            // Add Quartz builder.Services
            builder.Services.AddSingleton<IJobFactory, SingletonJobFactory>();
            builder.Services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != "Development")
            {
                if (FT_ManagementContext.ObterParam("EnvioEmailFerias", builder.Configuration.GetConnectionString("DefaultConnection")) == "1")
                {
                    // Add our job
                    builder.Services.AddSingleton<CronJobFerias>();
                    builder.Services.AddSingleton(new JobSchedule(
                        jobType: typeof(CronJobFerias),
                        cronExpression: FT_ManagementContext.ObterParam("DataEnvioEmailFerias", builder.Configuration.GetConnectionString("DefaultConnection"))));
                }

                if (FT_ManagementContext.ObterParam("EnvioEmailAgendamentoComercial", builder.Configuration.GetConnectionString("DefaultConnection")) == "1")
                {
                    builder.Services.AddSingleton<CronJobAgendamentoCRM>();
                    builder.Services.AddSingleton(new JobSchedule(
                        jobType: typeof(CronJobAgendamentoCRM),
                        cronExpression: FT_ManagementContext.ObterParam("DataEnvioEmailAgendamentoComercial", builder.Configuration.GetConnectionString("DefaultConnection"))));
                }


                if (FT_ManagementContext.ObterParam("EnvioEmailAniversario", builder.Configuration.GetConnectionString("DefaultConnection")) == "1")
                {
                    builder.Services.AddSingleton<CronJobAniversario>();
                    builder.Services.AddSingleton(new JobSchedule(
                        jobType: typeof(CronJobAniversario),
                        cronExpression: FT_ManagementContext.ObterParam("DataEnvioEmailAniversario", builder.Configuration.GetConnectionString("DefaultConnection"))));
                }

                if (FT_ManagementContext.ObterParam("SaidaAutomatica", builder.Configuration.GetConnectionString("DefaultConnection")) == "1")
                {
                    builder.Services.AddSingleton<CronJobSaida>();
                    builder.Services.AddSingleton(new JobSchedule(
                        jobType: typeof(CronJobSaida),
                        cronExpression: FT_ManagementContext.ObterParam("DataSaidaAutomatica", builder.Configuration.GetConnectionString("DefaultConnection"))));
                }
            }

            //COPIAR IMAGENS UTILIZADOR && APAGAR FILES DA PASTE TEMPORARIA
            FicheirosContext.GestaoFicheiros(true, true);

            builder.Services.AddHostedService<QuartzHostedService>();

            builder.Services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(cookieOptions =>
            {
                cookieOptions.LoginPath = "/Utilizadores/Login";
                cookieOptions.AccessDeniedPath = "/Home/AcessoNegado";
            });

            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

#if !DEBUG
            builder.Services.AddDataProtection().SetApplicationName("FT_Management").PersistKeysToFileSystem(new DirectoryInfo("/https/"));
                 builder.Services.AddLettuceEncrypt().PersistDataToDirectory(new DirectoryInfo("/https/"), "Password123");
#endif

            var app = builder.Build();

            System.Globalization.CultureInfo customCulture = new CultureInfo("pt-PT");
            customCulture.NumberFormat.NumberDecimalSeparator = ".";

            CultureInfo.DefaultThreadCurrentCulture = customCulture;

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.ConfigureExceptionHandler(app.Logger, app.Services.GetRequiredService<IHttpContextAccessor>());
            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                             name: "default",
                             pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Logger.LogDebug("Modo de DEBUG Ativo. Este modo irá gerar mais mensagens de informação!");

            app.Lifetime.ApplicationStarted.Register(() =>
            {
                app.Logger.LogInformation("WebApp Iniciada. .NET: {1}. Aplicação desenvolvida por {2} - {3}", app.GetType().Assembly.GetName().Version.ToString(), "Jorge Monteiro", "JKSProds - Software");
            });

            app.Lifetime.ApplicationStopping.Register(() =>
            {
                app.Logger.LogDebug("WebApp a parar todos os serviços ...");
            });

            app.Lifetime.ApplicationStopped.Register(() =>
            {
                app.Logger.LogInformation("WebApp parada com sucesso!");
            });

            app.Run();
        }
    }
}
