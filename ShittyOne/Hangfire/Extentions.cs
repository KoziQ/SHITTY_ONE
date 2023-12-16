using Hangfire;
using Hangfire.InMemory;

namespace ShittyOne.Hangfire
{
    public static class JobsExtensions
    {
        public static JobConfiguration AddJobManager(this IServiceCollection services)
        {
            services.AddHangfire(config =>
            {
                config.UseStorage(new InMemoryStorage());
            });
            services.AddHangfireServer();
            services.AddScoped<RecurringJobManager>();

            return new JobConfiguration(services);
        }

        public static IApplicationBuilder StartRecurringJobs(this IApplicationBuilder app)
        {
            var manager = app.ApplicationServices.CreateScope().ServiceProvider.GetService<RecurringJobManager>();
            manager.Start();
            return app;
        }
    }
}
