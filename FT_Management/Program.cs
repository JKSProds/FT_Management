using Microsoft.Extensions.Configuration;
using System.Globalization;

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

            //COPIAR IMAGENS UTILIZADOR && APAGAR FILES DA PASTE TEMPORARIA
            FicheirosContext.GestaoFicheiros(true, true);

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

            // Add Quartz builder.Services
            builder.Services.AddSingleton<IJobFactory, SingletonJobFactory>();
            builder.Services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

            if (FT_ManagementContext.ObterParam("FecharAcessoAutomatico", builder.Configuration.GetConnectionString("DefaultConnection")) == "1")
            {
                builder.Services.AddSingleton<CronJobAcessos>();
                builder.Services.AddSingleton(new JobSchedule(
                    jobType: typeof(CronJobAcessos),
                    cronExpression: FT_ManagementContext.ObterParam("DataFecharAcessoAutomatico", builder.Configuration.GetConnectionString("DefaultConnection"))));
            }

            if (FT_ManagementContext.ObterParam("EnviarAcessoAutomatico", builder.Configuration.GetConnectionString("DefaultConnection")) == "1")
            {
                builder.Services.AddSingleton<CronJobEnviarAcessos>();
                builder.Services.AddSingleton(new JobSchedule(
                    jobType: typeof(CronJobEnviarAcessos),
                    cronExpression: FT_ManagementContext.ObterParam("DataEnviarAcessoAutomatico", builder.Configuration.GetConnectionString("DefaultConnection"))));
            }

            builder.Services.AddHostedService<QuartzHostedService>();
#if !DEBUG
            builder.Services.AddDataProtection().SetApplicationName("ASGO_Management").PersistKeysToFileSystem(new DirectoryInfo("/https/"));
            builder.Services.AddLettuceEncrypt().PersistDataToDirectory(new DirectoryInfo("/https/"), "Password123");
#endif

            var app = builder.Build();

            CultureInfo cultura = new CultureInfo("pt-PT");
            CultureInfo.DefaultThreadCurrentCulture = cultura;
            CultureInfo.DefaultThreadCurrentUICulture = cultura;

            cultura.NumberFormat.NumberDecimalSeparator = ".";

            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

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
