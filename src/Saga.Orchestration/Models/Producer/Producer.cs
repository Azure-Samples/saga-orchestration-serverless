using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace Saga.Orchestration.Models.Producer
{
    public class Producer
    {
        private static readonly int MaxRetryAttempts = int.Parse(Environment.GetEnvironmentVariable("EventHubsProducerMaxRetryAttempts"));
        private static readonly int ExceptionsAllowed = int.Parse(Environment.GetEnvironmentVariable("EventHubsProducerExceptionsAllowedBeforeBreaking"));
        private static readonly int BreakDuration = int.Parse(Environment.GetEnvironmentVariable("EventHubsProducerBreakDuration"));

        public IAsyncCollector<EventData> MessagesCollector { get; set; }
        public ILogger Logger { get; set; }
        private static AsyncCircuitBreakerPolicy circuitBreakerPolicy;
        private static AsyncRetryPolicy retryPolicy;

        public Producer(IAsyncCollector<EventData> messagesCollector, ILogger logger)
        {
            MessagesCollector = messagesCollector;
            Logger = logger;

            circuitBreakerPolicy = CreateCircuitBreakerPolicy();
            retryPolicy = CreateRetryPolicy();
        }

        public static EventData CreateEventData<T>(T message)
        {
            var transactionString = JsonConvert.SerializeObject(message);
            byte[] messageBytes = Encoding.UTF8.GetBytes(transactionString);

            return new EventData(messageBytes);
        }

        public async Task<ProducerResult> ProduceCommandWithRetryAsync<T>(T message)
        {
            try
            {
                return await retryPolicy
                .WrapAsync(circuitBreakerPolicy)
                .ExecuteAsync(async () =>
                {
                    EventData eventData = CreateEventData(message);
                    await MessagesCollector.AddAsync(eventData);

                    return new ProducerResult
                    {
                        Message = eventData
                    };
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(string.Format(ConstantStrings.ProducerErrorPosting, ex.Message));

                return new ProducerResult
                {
                    Valid = false
                };
            }
        }

        private AsyncCircuitBreakerPolicy CreateCircuitBreakerPolicy()
        {
            return Policy
              .Handle<Exception>()
              .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: ExceptionsAllowed,
                durationOfBreak: TimeSpan.FromSeconds(BreakDuration));
        }

        private AsyncRetryPolicy CreateRetryPolicy()
        {
            return Policy
              .Handle<Exception>()
              .RetryAsync(MaxRetryAttempts, onRetry: (exception, retryCount) =>
              {
                  Logger.LogWarning(string.Format(ConstantStrings.ProducerErrorWithAttempts, retryCount, exception));
              });
        }
    }
}
