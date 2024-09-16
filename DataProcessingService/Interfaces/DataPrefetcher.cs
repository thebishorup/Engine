using DataProcessingService.Models;

using Dapper;
using System.Data.SqlClient;


namespace DataProcessingService.Interfaces;

public class DataPrefetcher : IDataPrefetcher
{
    private readonly string _connectionString;

    public DataPrefetcher(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<int> GetTotalAccountCountAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        string sql = "SELECT COUNT(*) FROM Accounts";
        return await connection.ExecuteScalarAsync<int>(sql);
    }

    public async Task<List<AccountData>> FetchAccountsAsync(int lastAccountId, int batchSize)
    {
        using var connection = new SqlConnection(_connectionString);
        string sql = @"
            SELECT AccountId, AccountName
            FROM Accounts
            WHERE AccountId > @LastAccountId
            ORDER BY AccountId
            OFFSET 0 ROWS
            FETCH NEXT @BatchSize ROWS ONLY";

        var accounts = await connection.QueryAsync<AccountData>(sql, new { LastAccountId = lastAccountId, BatchSize = batchSize });
        return accounts.ToList();
    }

    public async Task<List<PrefetchedData>> PrefetchDataAsync(List<AccountData> accounts)
    {
        using var connection = new SqlConnection(_connectionString);
        var accountIds = accounts.Select(a => a.AccountId).ToList();

        // Fetch AccountDetails
        string sql = @"
            SELECT AccountId, /* other columns */
            FROM AccountDetails
            WHERE AccountId IN @AccountIds";

        //var accountDetails = await connection.QueryAsync<AccountDetails>(sql, new { AccountIds = accountIds });

        //var accountDetailsDict = accountDetails.ToDictionary(ad => ad.AccountId);

        var prefetchedDataList = accounts.Select(account =>
        {
            //accountDetailsDict.TryGetValue(account.AccountId, out var details);
            return new PrefetchedData
            {
                AccountData = account
                //AccountDetails = details
            };
        }).ToList();

        return prefetchedDataList;
    }
}


