using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ActivityFeedPipeline.Web.Core;
using JetBrains.Annotations;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ActivityFeedPipeline.Web.Pages
{
	public class IndexModel : PageModel
	{
		private readonly IMediator mediator;

		public IndexModel([NotNull] IMediator mediator)
		{
			this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
		}

		public IReadOnlyList<ActivityFeedQueueItem> FeedItems { get; private set; }

		public async Task OnGet()
		{
			FeedItems = await mediator.Send(new GetActivityFeedCommand());
		}
	}
}