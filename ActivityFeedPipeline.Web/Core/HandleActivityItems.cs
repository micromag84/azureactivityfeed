using System;
using System.Threading;
using System.Threading.Tasks;
using ActivityFeedPipeline.Web.CosmosDb;
using ActivityFeedPipeline.Web.Storage;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ActivityFeedPipeline.Web.Core
{
	public class HandleActivityItemsCommand : IRequest
	{
	}

	[UsedImplicitly]
	public class HandleActivityItemsCommandHandler : IRequestHandler<HandleActivityItemsCommand>
	{
		private readonly IDocumentTypedCollectionClient<ActivityFeedQueueItem> documentClient;
		private readonly ILogger<HandleActivityItemsCommandHandler> logger;
		private readonly IStorageQueueClient queueClient;

		public HandleActivityItemsCommandHandler([NotNull] IStorageQueueClient queueClient, [NotNull] IDocumentTypedCollectionClient<ActivityFeedQueueItem> documentClient, [NotNull] ILogger<HandleActivityItemsCommandHandler> logger)
		{
			this.queueClient = queueClient ?? throw new ArgumentNullException(nameof(queueClient));
			this.documentClient = documentClient ?? throw new ArgumentNullException(nameof(documentClient));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task<Unit> Handle(HandleActivityItemsCommand notification, CancellationToken cancellationToken)
		{
			var messages = await queueClient.DequeueMessagesAsync<ActivityFeedQueueItem>("activityfeed-items", batchSize: 10);
			if (messages.IsEmpty)
			{
				return Unit.Value;
			}

			foreach (var message in messages)
			{
				logger.LogDebug($"Handling {nameof(ActivityFeedQueueItem)}s");
				message.Created = DateTime.UtcNow;
				await documentClient.CreateAsync(message);
			}

			await messages.DeleteBatch();
			logger.LogInformation($"{nameof(ActivityFeedQueueItem)} messages handled and deleted.");
			return Unit.Value;
		}
	}
}