using System.ComponentModel.DataAnnotations;

namespace SupportDesk.Api.Contracts;

public class CreateTicketRequest
{
    [Required]
    [MinLength(3)]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
}