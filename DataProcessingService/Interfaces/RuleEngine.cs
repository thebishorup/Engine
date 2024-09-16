using DataProcessingService.Models;
using Dapper;
using System.Data.SqlClient;
namespace DataProcessingService.Interfaces;

public class RuleEngine : IRuleEngine
{
    private readonly ICacheService _cacheService;
    private readonly string _connectionString;

    private const string RulesCacheKey = "RuleEngine_Rules";

    public RuleEngine(ICacheService cacheService, IConfiguration configuration)
    {
        _cacheService = cacheService;
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<List<Rule>> GetRulesAsync()
    {
        return await _cacheService.GetOrAddAsync(RulesCacheKey, async cacheEntry =>
        {
            cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

            using var connection = new SqlConnection(_connectionString);
            string sql = "SELECT Id, Name, Expression FROM Rules";
            var rules = await connection.QueryAsync<Rule>(sql);
            return rules.ToList();
        });
    }

    public void InvalidateRulesCache()
    {
        _cacheService.Remove(RulesCacheKey);
    }

    public async Task<List<RuleResult>> ApplyRulesAsync(List<PrefetchedData> dataBatch)
    {
        var rules = await GetRulesAsync();

        var ruleResults = new List<RuleResult>();

        foreach (var data in dataBatch)
        {
            var isValid = true;
            var validationMessages = new List<string>();

            foreach (var rule in rules)
            {
                var result = ApplyRule(rule, data);
                if (!result.IsValid)
                {
                    isValid = false;
                    validationMessages.Add(result.Message);
                }
            }

            ruleResults.Add(new RuleResult
            {
                AccountId = data.AccountData.AccountId,
                IsValid = isValid,
                ValidationMessage = string.Join("; ", validationMessages)
            });
        }

        return ruleResults;
    }

    private RuleValidationResult ApplyRule(Rule rule, PrefetchedData data)
    {
        // Implement your rule logic here
        bool isValid = true;
        string message = string.Empty;

        // Example rule logic
        //if (rule.Name == "MinimumBalance")
        //{
        //    if (data.AccountDetails.Balance < Convert.ToDecimal(rule.Expression))
        //    {
        //        isValid = false;
        //        message = $"Balance below minimum of {rule.Expression}.";
        //    }
        //}

        return new RuleValidationResult
        {
            IsValid = isValid,
            Message = message
        };
    }
}
