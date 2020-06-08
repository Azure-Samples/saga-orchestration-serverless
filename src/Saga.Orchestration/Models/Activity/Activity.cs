using System;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace Saga.Orchestration.Models.Activity
{
    public class Activity<T>
    {
        public string FunctionName { get; set; }
        public T Input { get; set; }
        public IDurableOrchestrationContext Context { get; set; }
        public TimeSpan Timeout { get; set; }
    }
}
