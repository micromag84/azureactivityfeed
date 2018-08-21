using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ActivityFeedPipeline.Web.CosmosDb;
using JetBrains.Annotations;
using MediatR;

namespace ActivityFeedPipeline.Web.Core
{
	public class GetActivityFeedCommand : IRequest<IReadOnlyList<ActivityFeedQueueItem>>
	{
	}

	[UsedImplicitly]
	public class GetActivityFeedCommandHandler : IRequestHandler<GetActivityFeedCommand, IReadOnlyList<ActivityFeedQueueItem>>
	{
		[NotNull]
		private readonly IDocumentTypedCollectionClient<ActivityFeedQueueItem> documentClient;

		public GetActivityFeedCommandHandler([NotNull] IDocumentTypedCollectionClient<ActivityFeedQueueItem> documentClient)
		{
			this.documentClient = documentClient ?? throw new ArgumentNullException(nameof(documentClient));
		}

		public async Task<IReadOnlyList<ActivityFeedQueueItem>> Handle(GetActivityFeedCommand request, CancellationToken cancellationToken)
		{
			var activityFeedQueueItems = await documentClient.GetAllAsync(item => true);
			return activityFeedQueueItems.OrderByDescending(item => item.Created).ToList();
		}
	}
}