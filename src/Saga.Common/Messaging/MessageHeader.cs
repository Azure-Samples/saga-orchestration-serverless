using System;
using Saga.Common.Utils;

namespace Saga.Common.Messaging
{
    public class MessageHeader
    {
        /// <summary>
        /// A unique identier that spans the whole transaction
        /// </summary>
        public string TransactionId { get; }

        /// <summary>
        /// A unique identier per message
        /// </summary>
        public string MessageId { get; }

        /// <summary>
        /// A message type used by producers/consumers to identify events and commands
        /// </summary>
        public string MessageType { get; }

        /// <summary>
        /// The name of the service that is sending the message
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// The creation date of the message
        /// </summary>
        public DateTime CreationDate { get; }


        public MessageHeader(string transactionId, string messageType, string source)
        {
            TransactionId = transactionId;
            MessageId = Guid.NewGuid().ToString();
            MessageType = messageType;
            Source = source;
            CreationDate = SystemTime.Now;
        }
    }
}
