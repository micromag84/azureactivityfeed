using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ActivityFeedPipeline.Web.Pages
{
	public class AboutModel : PageModel
	{
		public string Message { get; set; }

		public void OnGet()
		{
			Message = "Async activity feed pipeline in Azure.";
		}
	}
}