namespace DataProcessingService.Models;

public class RuleResult
{
    public int AccountId { get; set; }
    public bool IsValid { get; set; }
    public string ValidationMessage { get; set; }
}

