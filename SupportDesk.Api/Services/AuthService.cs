using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SupportDesk.Api.Contracts;
using SupportDesk.Api.Data;
using SupportDesk.Api.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SupportDesk.Api.Services;

public class AuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly JwtOptions _jwtOptions;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        AppDbContext dbContext,
        IPasswordHasher<User> passwordHasher,
        IOptions<JwtOptions> jwtOptions,
        ILogger<AuthService> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _dbContext.Users
            .SingleOrDefaultAsync(u => u.Username == request.Username);

        if (user is null)
        {
            _logger.LogWarning("Login failed for username {Username}", request.Username);
            return null;
        }

        var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            _logger.LogWarning("Login failed for username {Username}", request.Username);
            return null;
        }

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Username),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, user.Role)
        };

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));

        var signingCredentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogInformation(
            "Login succeeded for username {Username}. Token expires at {ExpiresAtUtc}",
            user.Username,
            expiresAtUtc);

        return new LoginResponse
        {
            AccessToken = accessToken,
            ExpiresAtUtc = expiresAtUtc
        };
    }

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _dbContext.Users
            .AnyAsync(u => u.Username == request.Username);

        if (existingUser)
        {
            _logger.LogWarning(
                "Registration failed because username {Username} already exists",
                request.Username);

            return RegisterResult.UsernameAlreadyExists;
        }

        var user = new User
        {
            Username = request.Username,
            Role = "User",
            CreatedAtUtc = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Registration succeeded for username {Username}",
            user.Username);

        return RegisterResult.Success;
    }
}