namespace ShittyOne.Hangfire.Jobs;

public interface IRecurringJob
{
    string CronExpression { get; }

    string JobId { get; }

    Task Execute();
}