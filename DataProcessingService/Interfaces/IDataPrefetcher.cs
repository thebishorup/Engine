using DataProcessingService.Models;

using Microsoft.Extensions.Configuration;


namespace DataProcessingService.Interfaces;
public interface IDataPrefetcher
{
    Task<int> GetTotalAccountCountAsync();
    Task<List<AccountData>> FetchAccountsAsync(int lastAccountId, int batchSize);
    Task<List<PrefetchedData>> PrefetchDataAsync(List<AccountData> accounts);
}

