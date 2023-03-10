using Microsoft.AspNetCore.Diagnostics;

namespace FT_Management

{
    public static class ServiceExtensions
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app, ILogger _logger, IHttpContextAccessor _httpContextAccessor)
        {
            FT_ManagementContext dbContext = new FT_ManagementContext(Custom.ConfigurationManager.AppSetting["ConnectionStrings:DefaultConnection"]);

            app.UseExceptionHandler(error =>
            {
                error.Run(async context =>
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/json";
                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        string e = Guid.NewGuid().ToString();
                        _logger.LogError(new EventId(), contextFeature.Error.Message, null);
                        MailContext.EnviarEmailError(dbContext.ObterUtilizador(int.Parse(_httpContextAccessor.HttpContext.User.Claims.First().Value)), e, contextFeature.Error.Message + "<br><br>" + contextFeature.Error.StackTrace.ToString());

                        context.Response.Redirect("/Home/Error/" + e, true);
                        await context.Response.WriteAsync(new Error
                        {
                            StatusCode = context.Response.StatusCode,
                            Message = "Internal Server Error. Please Try Again Later."
                        }.ToString());
                    }
                });
            });
        }
    }
}