using System;
using System.Threading;
using System.Threading.Tasks;
using ActivityFeedPipeline.Web.CosmosDb;
using ActivityFeedPipeline.Web.Storage;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ActivityFeedPipeline.Web.Core
{
	[UsedImplicitly]
	public class ActivityItemsHostedService : BackgroundService
	{
		private readonly TimeSpan delayInterval = TimeSpan.FromSeconds(10);
		private readonly ILogger<ActivityItemsHostedService> logger;
		private readonly IServiceScopeFactory scopeFactory;
		private DateTime lastTimeILogged = DateTime.MinValue;

		public ActivityItemsHostedService(
			[NotNull] IServiceScopeFactory scopeFactory,
			[NotNull] ILogger<ActivityItemsHostedService> logger)
		{
			this.scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				LogThatYoureStillAround();
				try
				{
					using (var scope = scopeFactory.CreateScope())
					{
						var queueClient = scope.ServiceProvider.GetService<IStorageQueueClient>();
						var documentClient = scope.ServiceProvider.GetService<IDocumentTypedCollectionClient<ActivityFeedQueueItem>>();

						// wait at least delay or longer if the publish takes longer.
						await Task.WhenAll(
							HandleItemsAsync(queueClient, documentClient),
							Task.Delay(delayInterval, stoppingToken));
					}
				}
				catch (TaskCanceledException)
				{
					logger.LogInformation("ActivityItemsHostedService is being shut down.");
				}
				catch (Exception e)
				{
					logger.LogError(e, $"Final catch in the ActivityItemsHostedService loop!");
				}
			}
		}

		private async Task HandleItemsAsync(IStorageQueueClient queueClient, IDocumentTypedCollectionClient<ActivityFeedQueueItem> documentClient)

		{
			var messages = await queueClient.DequeueMessagesAsync<ActivityFeedQueueItem>("activityfeed-items", batchSize: 10);
			if (messages.IsEmpty)
			{
				return;
			}

			foreach (var message in messages)
			{
				logger.LogDebug($"Handling {nameof(ActivityFeedQueueItem)}s");
				message.Created = DateTime.UtcNow;
				await documentClient.CreateAsync(message);
			}

			await messages.DeleteBatch();
			logger.LogInformation("ActivityFeedQueueItem messages handled and deleted.");
		}

		private void LogThatYoureStillAround()
		{
			var now = DateTime.UtcNow;
			if (now - lastTimeILogged <= TimeSpan.FromMinutes(1))
			{
				return;
			}

			lastTimeILogged = now;
			logger.LogDebug("ActivityItemsHostedService doing background work.");
		}
	}
}