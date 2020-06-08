using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Saga.Functions.Factories
{
    public class DurableClientFactory
    {
        private readonly HttpClient httpClient;
        private ILogger logger;

        public DurableClientFactory(HttpClient httpClient, ILogger log)
        {
            this.httpClient = httpClient;
            logger = log;
        }

        public async Task<string> GetRuntimeStatusAsync(HttpResponseMessage clientResponse)
        {
            string statusContent = await clientResponse.Content.ReadAsStringAsync();
            HttpManagementPayload managementPayload = JsonConvert.DeserializeObject<HttpManagementPayload>(statusContent);

            return await GetOrchestratorStatusAsync(managementPayload.StatusQueryGetUri, logger);
        }

        private async Task<string> GetOrchestratorStatusAsync(string orchestratorStatusUri, ILogger log)
        {
            try
            {
                HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(orchestratorStatusUri);
                var content = await httpResponseMessage.Content.ReadAsStringAsync();
                var orchestrationStatus = JsonConvert.DeserializeObject<DurableOrchestrationStatus>(content);
                var orchestrationRuntimeStatus = Enum.GetName(typeof(OrchestrationRuntimeStatus), orchestrationStatus.RuntimeStatus);

                log.LogInformation($@"Orchestration runtime status found => {orchestrationRuntimeStatus}");

                return orchestrationRuntimeStatus;
            }
            catch (HttpRequestException ex)
            {
                log.LogError($@"Orchestration runtime not found => {ex.Message}");
                return nameof(OrchestrationRuntimeStatus.Unknown);
            }
        }
    }
}
