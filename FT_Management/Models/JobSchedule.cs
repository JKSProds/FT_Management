using Microsoft.Extensions.Configuration;

[DisallowConcurrentExecution]
public class CronJobAcessos : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        try
        {
            FT_ManagementContext dbContext = new FT_ManagementContext(Custom.ConfigurationManager.AppSetting["ConnectionStrings:DefaultConnection"]);
            List<Acesso> LstAcessos = new List<Acesso>();
            foreach(var u in dbContext.ObterListaUtilizadores(true, false).Where(u => u.Acessos && u.AcessoAtivo)) { LstAcessos.Add(new Acesso(){
                    IdUtilizador = u.Id,
                    Data = DateTime.Now,
                    Tipo = 2,
                    Temperatura = "",
                    Utilizador = u
                    });} 
                    
            dbContext.CriarAcessoInterno(LstAcessos);
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
            
            DateTime dI = DateTime.Parse(DateTime.Now.AddMonths(-1).ToString("yyyy/MM")+ "/23");
            DateTime dF = DateTime.Parse(DateTime.Now.ToString("yyyy/MM")+ "/22");

            List<Utilizador> lstU = dbContext.ObterListaUtilizadores(true, false).Where(u => u.Admin).ToList();
            Attachment a = new Attachment((new MemoryStream(dbContext.GerarMapaPresencas(dI,dF).ToArray())), "MapaPresencas_" + dI.ToString("ddMMyy") + "_" + dF.ToString("ddMMyy") +".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            
            foreach (var u in lstU) {
                MailContext.EnviarEmailAcessos(dI, dF, u, a);
            }
            MailContext.EnviarEmailAcessos(dI, dF, dbContext.ObterUtilizador(10), new Attachment((new MemoryStream(dbContext.GerarMapaPresencas(dI,dF).ToArray())), "MapaPresencas_" + dI.ToString("ddMMyy") + "_" + dF.ToString("ddMMyy") +".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
          
        }
        catch
        {
        }
        return Task.CompletedTask;
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