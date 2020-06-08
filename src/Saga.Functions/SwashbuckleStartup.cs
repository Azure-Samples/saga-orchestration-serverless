using AzureFunctions.Extensions.Swashbuckle;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using SagaOrchestration;
using System.Reflection;

[assembly: WebJobsStartup(typeof(SwashBuckleStartup))]
namespace SagaOrchestration
{
    internal class SwashBuckleStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            // Register the extension
            builder.AddSwashBuckle(Assembly.GetExecutingAssembly());
        }
    }
}
