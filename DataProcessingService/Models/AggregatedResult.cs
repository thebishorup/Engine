using System.ComponentModel.DataAnnotations;

namespace DataProcessingService.Models;

public class AggregatedResult
{
    [Key]
    public int AccountId { get; set; }
    public bool IsValid { get; set; }
    public string ValidationMessage { get; set; }
    public DateTime ProcessedAt { get; set; }
}

