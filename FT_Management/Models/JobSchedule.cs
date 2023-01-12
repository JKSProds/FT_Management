using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Quartz.Spi;
using System;
using System.Threading;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using FT_Management.Models;
using System.IO;
using Microsoft.Extensions.Configuration;
using Custom;
using System.Linq;

[DisallowConcurrentExecution]
public class CronJobFerias : IJob
{


    public Task Execute(IJobExecutionContext context)
    {
        //_logger.LogInformation("Hello world!");
        try
        {

            FT_ManagementContext dbContext = new FT_ManagementContext(ConfigurationManager.AppSetting["ConnectionStrings:DefaultConnection"], "");
            List<Ferias> LstFerias = dbContext.ObterListaFeriasValidar();

            if (LstFerias.Count > 0)
            {
                MailContext.EnviarEmailFeriasPendentes("geral@food-tech.pt", LstFerias);
            }
            dbContext.ValidarEmailEnviado();
        }
        catch
        {
        }
        return Task.CompletedTask;
    }
}

public class CronJobAniversario : IJob
{


    public Task Execute(IJobExecutionContext context)
    {
        //_logger.LogInformation("Hello world!");
        try
        {

            FT_ManagementContext dbContext = new FT_ManagementContext(ConfigurationManager.AppSetting["ConnectionStrings:DefaultConnection"], "");
            List<Utilizador> LstUtilizadores = dbContext.ObterListaUtilizadores(false, false).Where(u => u.DataNascimento.ToString("yyyy") != "0001").Where(u => u.DataNascimento.ToString("dd-MM") == DateTime.Now.ToString("dd-MM")).ToList();

            if (LstUtilizadores.Count > 0)
            {
                MailContext.EnviarEmailAniversario(LstUtilizadores);
            }
            dbContext.ValidarEmailEnviado();
        }
        catch
        {
        }
        return Task.CompletedTask;
    }
}

public class CronJobAgendamentoCRM : IJob
{


    public Task Execute(IJobExecutionContext context)
    {
        //_logger.LogInformation("Hello world!");
        try
        {
            FT_ManagementContext dbContext = new FT_ManagementContext(ConfigurationManager.AppSetting["ConnectionStrings:DefaultConnection"], "");

            foreach (var u in dbContext.ObterListaUtilizadores(true, false))
            {
                List<Visita> LstVisitas = dbContext.ObterListaVisitas(u.Id, DateTime.Parse(DateTime.Now.ToShortDateString()), DateTime.Parse(DateTime.Now.ToShortDateString()));

                if (LstVisitas.Count > 0)
                {
                    MailContext.EnviarEmailMarcacaoDiariaComercial(u, LstVisitas);
                }
            }
        }
        catch
        {
        }
        return Task.CompletedTask;
    }
}


namespace Custom
{
    static class ConfigurationManager
    {
        public static IConfiguration AppSetting { get; }
        static ConfigurationManager()
        {
            AppSetting = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();
        }
    }
}

public class SingletonJobFactory : IJobFactory
{
    private readonly IServiceProvider _serviceProvider;
    public SingletonJobFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        return _serviceProvider.GetRequiredService(bundle.JobDetail.JobType) as IJob;
    }

    public void ReturnJob(IJob job) { }
}


public class JobSchedule
{
    public JobSchedule(Type jobType, string cronExpression)
    {
        JobType = jobType;
        CronExpression = cronExpression;
    }

    public Type JobType { get; }
    public string CronExpression { get; }
}

public class QuartzHostedService : IHostedService
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly IJobFactory _jobFactory;
    private readonly IEnumerable<JobSchedule> _jobSchedules;

    public QuartzHostedService(
        ISchedulerFactory schedulerFactory,
        IJobFactory jobFactory,
        IEnumerable<JobSchedule> jobSchedules)
    {
        _schedulerFactory = schedulerFactory;
        _jobSchedules = jobSchedules;
        _jobFactory = jobFactory;
    }
    public IScheduler Scheduler { get; set; }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        Scheduler.JobFactory = _jobFactory;

        foreach (var jobSchedule in _jobSchedules)
        {
            var job = CreateJob(jobSchedule);
            var trigger = CreateTrigger(jobSchedule);

            await Scheduler.ScheduleJob(job, trigger, cancellationToken);
        }

        await Scheduler.Start(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Scheduler?.Shutdown(cancellationToken);
    }

    private static IJobDetail CreateJob(JobSchedule schedule)
    {
        var jobType = schedule.JobType;
        return JobBuilder
            .Create(jobType)
            .WithIdentity(jobType.FullName)
            .WithDescription(jobType.Name)
            .Build();
    }

    private static ITrigger CreateTrigger(JobSchedule schedule)
    {
        return TriggerBuilder
            .Create()
            .WithIdentity($"{schedule.JobType.FullName}.trigger")
            .WithCronSchedule(schedule.CronExpression)
            .WithDescription(schedule.CronExpression)
            .Build();
    }
}