using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ActivityFeedPipeline.Web.CosmosDb
{
	public interface IDocumentTypedCollectionClient<T>
	{
		void Initialize();

		Task<IReadOnlyList<T>> GetAllAsync(Expression<Func<T, bool>> predicate);

		Task<T> GetAsync(string id);

		Task<T> CreateAsync(T item);

		Task<T> UpdateAsync(string id, T item);

		Task DeleteAsync(string id);
	}
}