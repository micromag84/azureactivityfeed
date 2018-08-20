using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using ActivityFeedPipeline.Web.Core;
using JetBrains.Annotations;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Document = Microsoft.Azure.Documents.Document;

namespace ActivityFeedPipeline.Web.CosmosDb
{
	[UsedImplicitly]
	internal class DocumentTypedCollectionClient<T> : IDocumentTypedCollectionClient<T> where T : DocumentBase
	{
		private readonly DocumentClient documentClient;
		private readonly string collectionId;
		private readonly string databaseId;
		private readonly ILogger<DocumentTypedCollectionClient<T>> logger;

		public DocumentTypedCollectionClient([NotNull] IOptions<DocumentClientOptions> documentClientOptions, [NotNull] ILogger<DocumentTypedCollectionClient<T>> logger)
		{
			if (documentClientOptions == null)
			{
				throw new ArgumentNullException(nameof(documentClientOptions));
			}
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

			databaseId = documentClientOptions.Value.DatabaseId;
			collectionId = typeof(T).Name;
			documentClient = new DocumentClient(new Uri(documentClientOptions.Value.Endpoint), documentClientOptions.Value.Key);
		}

		public async Task<T> GetAsync(string id)
		{
			try
			{
				logger.LogDebug($"Loading item with id {id}");

				Document document = await documentClient.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, id));

				return ToDocumentBase(document);
			}
			catch (DocumentClientException e)
			{
				if (e.StatusCode == HttpStatusCode.NotFound)
				{
					return null;
				}

				logger.LogError(e, $"Error loading item with id {id}");
				throw;
			}
		}

		public async Task<IReadOnlyList<T>> GetAllAsync(Expression<Func<T, bool>> predicate)
		{
			try
			{
				logger.LogDebug("Loading items");

				var query = documentClient.CreateDocumentQuery<T>(
						UriFactory.CreateDocumentCollectionUri(databaseId, collectionId),
						new FeedOptions { MaxItemCount = -1})
					.Where(predicate)
					.AsDocumentQuery();

				var results = new List<T>();
				while (query.HasMoreResults)
				{
					results.AddRange(await query.ExecuteNextAsync<T>());
				}

				return results;
			}
			catch (Exception e)
			{
				logger.LogError(e, "Error loading items");
				throw;
			}
		}

		public async Task<T> CreateAsync(T item)
		{
			try
			{
				logger.LogDebug("Creating item");
				var document = await documentClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseId, collectionId), item);

				return ToDocumentBase(document);
			}
			catch (Exception e)
			{
				logger.LogError(e, "Error creating item");
				throw;
			}
		}

		public async Task<T> UpdateAsync(string id, T item)
		{
			try
			{
				logger.LogDebug($"Updating item with id {id}");

				var document = await documentClient.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, id), item);

				return ToDocumentBase(document);
			}
			catch (Exception e)
			{
				logger.LogError(e, $"Error updating item with id {id}");
				throw;
			}
		}

		public async Task DeleteAsync(string id)
		{
			try
			{
				logger.LogDebug($"Deleting item with id {id}");

				await documentClient.DeleteDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, id));
			}
			catch (Exception e)
			{
				logger.LogError(e, $"Error deleting item with id {id}");
				throw;
			}
		}

		public void Initialize()
		{
			documentClient.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseId }).Wait();
			logger.LogInformation($"Database {databaseId} available.");
			var documentCollection = new DocumentCollection { Id = collectionId };

			var requestOptions = new RequestOptions { OfferThroughput = 1000 };

			documentClient.CreateDocumentCollectionIfNotExistsAsync(
				UriFactory.CreateDatabaseUri(databaseId), documentCollection, requestOptions).Wait();
			logger.LogInformation($"Collection {collectionId} available.");
		}

		private static T ToDocumentBase(Document document)
		{
			return (T)(dynamic)document;
		}

	}
}
