using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Api.Contracts;
using SupportDesk.Api.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SupportDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly TicketService _ticketService;

    // DI injects TicketService here
    public TicketsController(TicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<TicketSummaryDto>>> GetTickets([FromQuery] GetTicketsQuery query)
    {
        return Ok(await _ticketService.GetPagedAsync(query));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TicketSummaryDto>> GetTicketById(int id)
{
    var ticket = await _ticketService.GetByIdAsync(id);
    if (ticket is null) return NotFound();
    return Ok(ticket);
}

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TicketSummaryDto>> CreateTicket([FromBody] CreateTicketRequest request)
    {
        var username = GetCurrentUsername();

        if (string.IsNullOrWhiteSpace(username))
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "User identity missing",
                Status = StatusCodes.Status401Unauthorized,
                Detail = "The authenticated user's name could not be determined from the token."
            });
        }

        var created = await _ticketService.CreateAsync(request.Title, username);

        return CreatedAtAction(
            nameof(GetTicketById),
            new { id = created.Id },
            created
        );
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> UpdateTicketStatus(
    int id,
    [FromBody] UpdateTicketStatusRequest request)
    {
        var username = GetCurrentUsername();
        var role = GetCurrentUserRole();

        if (string.IsNullOrWhiteSpace(username))
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "User identity missing",
                Status = StatusCodes.Status401Unauthorized,
                Detail = "The authenticated user's name could not be determined from the token."
            });
        }

        var result = await _ticketService.UpdateStatusAsync(id, request.Status, username, role);

        return result switch
        {
            TicketOperationResult.NotFound => NotFound(),
            TicketOperationResult.Forbidden => Forbid(),
            TicketOperationResult.Success => NoContent(),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        var username = GetCurrentUsername();
        var role = GetCurrentUserRole();

        if (string.IsNullOrWhiteSpace(username))
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "User identity missing",
                Status = StatusCodes.Status401Unauthorized,
                Detail = "The authenticated user's name could not be determined from the token."
            });
        }

        var result = await _ticketService.DeleteAsync(id, username, role);

        return result switch
        {
            TicketOperationResult.NotFound => NotFound(),
            TicketOperationResult.Forbidden => Forbid(),
            TicketOperationResult.Success => NoContent(),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    private string? GetCurrentUsername()
    {
        return User.FindFirst("unique_name")?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.Identity?.Name;
    }

    private string? GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value;
    }
}