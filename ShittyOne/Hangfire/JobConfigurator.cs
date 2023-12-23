using ShittyOne.Hangfire.Jobs;

namespace ShittyOne.Hangfire;

public class JobConfiguration
{
    private readonly IServiceCollection services;

    internal JobConfiguration(IServiceCollection services)
    {
        this.services = services;
    }

    public JobConfiguration AddrecurringJob<TJob>() where TJob : IRecurringJob
    {
        services.AddScoped(typeof(IRecurringJob), typeof(TJob));
        return this;
    }
}