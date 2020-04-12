using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pecuniary.TimeSeries.Services
{
    public interface IDynamoDbService
    {
        Task<ICollection<T>> GetAllAsync<T>();
        Task SaveToDynamoAsync(IEnumerable<Quotes> symbols);
    }
}