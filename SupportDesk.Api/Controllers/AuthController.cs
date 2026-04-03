using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportDesk.Api.Contracts;
using SupportDesk.Api.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SupportDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        return result switch
        {
            RegisterResult.UsernameAlreadyExists => Conflict(new ProblemDetails
            {
                Title = "Username already exists",
                Status = StatusCodes.Status409Conflict,
                Detail = "That username is already taken."
            }),
            RegisterResult.Success => StatusCode(StatusCodes.Status201Created),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        };
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);

        if (response is null)
        {
            return Unauthorized(new ProblemDetails
            {
                Title = "Invalid credentials",
                Status = StatusCodes.Status401Unauthorized,
                Detail = "The username or password is incorrect."
            });
        }

        return Ok(response);
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<CurrentUserResponse> Me()
    {
        var username =
            User.FindFirst(ClaimTypes.Name)?.Value ??
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            User.FindFirst("unique_name")?.Value ??
            User.FindFirst("sub")?.Value;

        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        var claims = User.Claims
            .Select(c => new ClaimResponse
            {
                Type = c.Type,
                Value = c.Value
            })
            .ToList();

        var response = new CurrentUserResponse
        {
            Username = username,
            Role = role,
            Claims = claims
        };

        return Ok(response);
    }
}