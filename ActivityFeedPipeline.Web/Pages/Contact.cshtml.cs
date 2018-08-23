using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ActivityFeedPipeline.Web.Pages
{
	public class ContactModel : PageModel
	{
		public string Message { get; set; }

		public void OnGet()
		{
			Message = "Contact me.";
		}
	}
}