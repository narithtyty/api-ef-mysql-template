using System;

public class JobSchedule
{
    public JobSchedule(string jobName, Type jobType, string cronExpression)
    {
        JobName = jobName;
        JobType = jobType;
        CronExpression = cronExpression;
    }

    public string JobName { get; }
    public Type JobType { get; }
    public string CronExpression { get; }
}