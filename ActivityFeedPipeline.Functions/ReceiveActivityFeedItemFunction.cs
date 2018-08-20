using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace ActivityFeedPipeline.Functions
{
	[UsedImplicitly]
	public static class ReceiveActivityFeedItemFunction
	{
		[FunctionName("ReceiveActivityFeedItem")]
		public static async Task<HttpResponseMessage> Run(
			[HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
			HttpRequestMessage request,
			[Queue("activityfeed-items", Connection = "AzureWebJobsStorage")]
			ICollector<string> outputQueue,
			TraceWriter log)
		{
			try
			{
				var feedItem = await request.Content.ReadAsAsync<ActivityFeedItem>();

				if (!feedItem.IsValid()) return new HttpResponseMessage(HttpStatusCode.BadRequest);

				Cleanup(feedItem);
				var serializeObject = JsonConvert.SerializeObject(feedItem);

				log.Info($"Adding external activity with id '{feedItem.Id}' to queue");
				outputQueue.Add(serializeObject);
			}
			catch (Exception e)
			{
				log.Error("Failed to receive incoming external activity item.", e);
				return new HttpResponseMessage(HttpStatusCode.InternalServerError);
			}

			return new HttpResponseMessage(HttpStatusCode.OK);
		}

		private static void Cleanup(ActivityFeedItem externalActivityFeed)
		{
			string GetTruncatedText(string text, int maxLength)
			{
				if (!string.IsNullOrEmpty(text))
				{
					text = text.Trim();
					text = text.Length <= maxLength ? text : text.Substring(0, maxLength) + "…";
				}

				return text;
			}

			externalActivityFeed.Title = GetTruncatedText(externalActivityFeed.Title, 120);
			externalActivityFeed.Text = GetTruncatedText(externalActivityFeed.Text, 280);
		}
	}
}