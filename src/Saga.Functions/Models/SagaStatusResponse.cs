using Newtonsoft.Json;

namespace Saga.Functions.Models
{
    public class SagaStatusResponse
    {
        [JsonProperty("status")]
        public SagaStatus Status { get; set; }


        public SagaStatusResponse(string sagaState, string orchestrationEngineState)
        {
            Status = new SagaStatus
            {
                SagaState = sagaState,
                OrchestrationEngineState = orchestrationEngineState
            };
        }
    }

    public class SagaStatus
    {
        [JsonProperty("saga")]
        public string SagaState { get; set; }

        [JsonProperty("orchestrationEngine")]
        public string OrchestrationEngineState { get; set; }
    }
}
