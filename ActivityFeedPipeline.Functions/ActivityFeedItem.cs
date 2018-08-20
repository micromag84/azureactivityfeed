using System;
using JetBrains.Annotations;

namespace ActivityFeedPipeline.Functions
{
	[UsedImplicitly]
	internal class ActivityFeedItem
	{
		public ActivityFeedItem()
		{
			Id = Guid.NewGuid().ToString();
		}

		public string Id { get; }

		public string Source { get; set; }

		public string Title { get; set; }

		public string Text { get; set; }

		public string MediaUrl { get; set; }

		public string Url { get; set; }


		public bool IsValid() => !string.IsNullOrEmpty(Source) && !string.IsNullOrEmpty(Title);
	}
}