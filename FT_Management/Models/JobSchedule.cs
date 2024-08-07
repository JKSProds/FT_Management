﻿using Microsoft.Extensions.Configuration;

[DisallowConcurrentExecution]
public class CronJobFerias : IJob
{


    public Task Execute(IJobExecutionContext context)
    {
        try
        {

            FT_ManagementContext dbContext = new FT_ManagementContext(Custom.ConfigurationManager.AppSetting["ConnectionStrings:DefaultConnection"]);
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
        try
        {

            FT_ManagementContext dbContext = new FT_ManagementContext(Custom.ConfigurationManager.AppSetting["ConnectionStrings:DefaultConnection"]);
            List<Utilizador> LstUtilizadores = dbContext.ObterListaUtilizadores(false, false).Where(u => u.DataNascimento.ToString("yyyy") != "0001").Where(u => u.DataNascimento.ToString("dd-MM") == DateTime.Now.AddDays(1).ToString("dd-MM")).ToList();

            if (LstUtilizadores.Count > 0)
            {
                MailContext.EnviarEmailAniversario(LstUtilizadores);
            }
        }
        catch
        {
        }
        return Task.CompletedTask;
    }
}

public class CronJobSaida : IJob
{


    public Task Execute(IJobExecutionContext context)
    {
        try
        {

            FT_ManagementContext dbContext = new FT_ManagementContext(Custom.ConfigurationManager.AppSetting["ConnectionStrings:DefaultConnection"]);
            List<Acesso> LstAcessos = new List<Acesso>();

            foreach (Utilizador u in dbContext.ObterListaUtilizadores(true, false).Where(u => u.AcessoAtivo && u.Acessos))
            {
                LstAcessos.Add(new Acesso()
                {
                    IdUtilizador = u.Id,
                    Data = DateTime.Parse(DateTime.Now.ToString("dd/MM/yyyy 18:30:00")),
                    Tipo = 2,
                    Temperatura = "Saida Automática - " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")
                });
            }
            dbContext.CriarAcessoInterno(LstAcessos);
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
        try
        {
            FT_ManagementContext dbContext = new FT_ManagementContext(Custom.ConfigurationManager.AppSetting["ConnectionStrings:DefaultConnection"]);

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

public class CronJobEnviarAcessos : IJob
{


    public Task Execute(IJobExecutionContext context)
    {
        try
        {
            FT_ManagementContext dbContext = new FT_ManagementContext(Custom.ConfigurationManager.AppSetting["ConnectionStrings:DefaultConnection"]);
            
            DateTime dI = DateTime.Parse(DateTime.Now.AddMonths(-1).ToString("yyyy/MM")+ "/01");
            DateTime dF = DateTime.Parse(DateTime.Now.AddMonths(-1).ToString("yyyy/MM")+ "/" + DateTime.DaysInMonth(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month).ToString());

            List<Utilizador> lstU = dbContext.ObterListaUtilizadores(true, false).Where(u => u.Admin).ToList();
            Attachment a = new Attachment((new MemoryStream(dbContext.GerarMapaPresencas(dI,dF).ToArray())), "MapaPresencas_" + dI.ToString("ddMMyy") + "_" + dF.ToString("ddMMyy") +".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            
            MailContext.EnviarEmailAcessos(dI, dF, dbContext.ObterUtilizador(26), new Attachment((new MemoryStream(dbContext.GerarMapaPresencas(dI,dF).ToArray())), "MapaPresencas_" + dI.ToString("ddMMyy") + "_" + dF.ToString("ddMMyy") +".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
          
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