using Hangfire;
using ShittyOne.Hangfire.Jobs;

namespace ShittyOne.Hangfire
{
    public class RecurringJobManager
    {
        private readonly IRecurringJobManager manager;
        private readonly IEnumerable<IRecurringJob> jobs;

        public RecurringJobManager(IRecurringJobManager manager, IEnumerable<IRecurringJob> jobs)
        {
            this.manager = manager;
            this.jobs = jobs;
        }

        public void Start()
        {
            foreach (var job in jobs)
            {
                manager.AddOrUpdate(job.JobId, () => job.Execute(), job.CronExpression);
            }
        }
    }
}
