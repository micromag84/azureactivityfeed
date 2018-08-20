using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace ActivityFeedPipeline.Web.Storage
{
	/// <summary>
	///     Returns a batch of messages. Gives you an API to communicate what to do with them
	/// </summary>
	public class MessageBatch<T> : IMessageBatch<T>
	{
		private readonly Func<CloudQueueMessage, Task> deleteTask;
		private readonly IEnumerable<CloudQueueMessage> messages;

		internal MessageBatch(IEnumerable<CloudQueueMessage> messages, Func<CloudQueueMessage, Task> deleteTask)
		{
			this.messages = messages;
			this.deleteTask = deleteTask;
		}

		public bool IsEmpty => !messages.Any();

		public IEnumerator<T> GetEnumerator()
			=> messages.Select(m => JsonConvert.DeserializeObject<T>(m.AsString)).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public async Task DeleteBatch()
		{
			foreach (var msg in messages)
			{
				await deleteTask(msg);
			}
		}
	}
}