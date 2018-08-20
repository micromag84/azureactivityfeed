using System.Collections.Generic;
using System.Threading.Tasks;

namespace ActivityFeedPipeline.Web.Storage
{
	/// <summary>
	///     Abstraction to interact with azure storage queues
	/// </summary>
	public interface IStorageQueueClient
	{
		/// <summary>
		///     Enqueue a message - the object will be serialized to Json and sent as queue message
		/// </summary>
		/// <param name="queueName">The name of the queue</param>
		/// <param name="message">The object to be sent as message</param>
		Task EnqueueMessageAsync<T>(string queueName, T message);

		/// <summary>
		///     Dequeue messages of the given type
		/// </summary>
		/// <param name="queueName">The name of the queue</param>
		/// <param name="batchSize">The size of the batch, means how many items are dequeued</param>
		Task<IMessageBatch<T>> DequeueMessagesAsync<T>(string queueName, int batchSize);
	}

	/// <summary>
	///     Returns a batch of messages. Gives you an API to communicate what to do with them
	/// </summary>
	public interface IMessageBatch<out T> : IEnumerable<T>
	{
		bool IsEmpty { get; }

		Task DeleteBatch();
	}
}