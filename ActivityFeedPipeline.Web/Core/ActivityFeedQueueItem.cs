using System;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace ActivityFeedPipeline.Web.Core
{
	[UsedImplicitly]
	public class ActivityFeedQueueItem : DocumentBase
	{
		public string Source { get; set; }

		public string Title { get; set; }

		[DataType(DataType.MultilineText)]
		public string Text { get; set; }

		public string MediaUrl { get; set; }

		[DataType(DataType.Url)]
		public string Url { get; set; }

		[DataType(DataType.DateTime)]
		public DateTime Created { get; set; }


	}
}