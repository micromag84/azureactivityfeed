using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ActivityFeedPipeline.Web.Core;
using ActivityFeedPipeline.Web.CosmosDb;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ActivityFeedPipeline.Web.Pages
{
	public class IndexModel : PageModel
	{
		private readonly IDocumentTypedCollectionClient<ActivityFeedQueueItem> documentClient;

		public IndexModel([NotNull] IDocumentTypedCollectionClient<ActivityFeedQueueItem> documentClient)
		{
			this.documentClient = documentClient ?? throw new ArgumentNullException(nameof(documentClient));
		}

		public IReadOnlyList<ActivityFeedQueueItem> FeedItems { get; private set; }

		public async Task OnGet()
		{
			var activityFeedQueueItems = await documentClient.GetAllAsync(item => true);
			FeedItems = activityFeedQueueItems.OrderByDescending(item => item.Created).ToList();
		}
	}
}