using System;

namespace Saga.Orchestration.Utils
{
    public static class TimeoutUtils
    {
        public static TimeSpan FormatAccountsValidationTimeout()
        {
            int timeoutSeconds = int.Parse(Environment.GetEnvironmentVariable("ValidatorTimeoutSeconds"));
            return TimeSpan.FromSeconds(timeoutSeconds);
        }

        public static TimeSpan FormatTransferTimeout()
        {
            int timeoutSeconds = int.Parse(Environment.GetEnvironmentVariable("TransferTimeoutSeconds"));
            return TimeSpan.FromSeconds(timeoutSeconds);
        }

        public static TimeSpan FormatReceiptTimeout()
        {
            int timeoutSeconds = int.Parse(Environment.GetEnvironmentVariable("ReceiptTimeoutSeconds"));
            return TimeSpan.FromSeconds(timeoutSeconds);
        }
    }
}
