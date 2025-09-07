using Hangfire;

namespace FunnyActivities.CrossCuttingConcerns.BackgroundJobs;

public class SampleJob
{
    [AutomaticRetry(Attempts = 3)]
    public void Execute()
    {
        // Sample background job logic
        Console.WriteLine("Background job executed at: " + DateTime.Now);
    }
}