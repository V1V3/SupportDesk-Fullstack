using System.ComponentModel.DataAnnotations;

namespace SupportDesk.Api.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class AllowedTicketStatusAttribute : ValidationAttribute
{
    private static readonly HashSet<string> AllowedStatuses = new()
    {
        "Open",
        "In Progress",
        "Completed",
        "Rejected"
    };

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
            return ValidationResult.Success;

        if (value is not string stringValue)
            return new ValidationResult("Invalid value type.");

        if (AllowedStatuses.Contains(stringValue))
            return ValidationResult.Success;

        return new ValidationResult(
            $"Status must be one of: {string.Join(", ", AllowedStatuses)}");
    }
}