using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace ActivityFeedPipeline.Web.Storage
{
	public class AzureStorageQueueClient : IStorageQueueClient
	{
		private static volatile bool doesQueueExist;
		private readonly string connectionString;
		private readonly ILogger<AzureStorageQueueClient> logger;

		public AzureStorageQueueClient([NotNull] string connectionString, [NotNull] ILogger<AzureStorageQueueClient> logger)
		{
			this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task EnqueueMessageAsync<T>(string queueName, T message)
		{
			var client = await GetQueueClient(queueName);
			await client.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(message)));
		}

		public async Task<IMessageBatch<T>> DequeueMessagesAsync<T>(string queueName, int batchSize)
		{
			var q = await GetQueueClient(queueName);
			var messages = await q.GetMessagesAsync(batchSize);
			return new MessageBatch<T>(messages, DeleteMessage);

			Task DeleteMessage(CloudQueueMessage msg) => q.DeleteMessageAsync(msg);
		}

		private async Task<CloudQueue> GetQueueClient([NotNull] string queueName)
		{
			if (queueName == null)
			{
				throw new ArgumentNullException(nameof(queueName));
			}

			if (!CloudStorageAccount.TryParse(connectionString, out var storageAccount))
			{
				throw new ArgumentException("unreadable connection string for blob storage", nameof(connectionString));
			}

			var queue = storageAccount.CreateCloudQueueClient().GetQueueReference(queueName);
			if (!doesQueueExist)
			{
				var isNewQueue = await queue.CreateIfNotExistsAsync();
				if (isNewQueue)
				{
					logger.LogInformation($"Queue with name {queueName} has been created");
				}

				doesQueueExist = true;
			}

			return queue;
		}
	}
}