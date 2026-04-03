using System.ComponentModel.DataAnnotations;
using SupportDesk.Api.Validation;

namespace SupportDesk.Api.Contracts;

public sealed class UpdateTicketStatusRequest
{
    [Required]
    [AllowedTicketStatus]
    public string Status { get; set; } = string.Empty;
}