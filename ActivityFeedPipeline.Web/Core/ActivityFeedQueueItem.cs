using System;
using JetBrains.Annotations;

namespace ActivityFeedPipeline.Web.Core
{
	[UsedImplicitly]
	public class ActivityFeedQueueItem : DocumentBase
	{
		public string Source { get; set; }

		public string Title { get; set; }

		public string Text { get; set; }

		public string MediaUrl { get; set; }

		public string Url { get; set; }

		public DateTime Created { get; set; }


	}
}