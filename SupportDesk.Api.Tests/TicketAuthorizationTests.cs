using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SupportDesk.Api.Contracts;

namespace SupportDesk.Api.Tests;

public class TicketAuthorizationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TicketAuthorizationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task NonOwner_Cannot_Update_Ticket()
    {
        // --- register user A ---
        var userA = new RegisterRequest
        {
            Username = "userA",
            Password = "Pass123!"
        };

        await _client.PostAsJsonAsync("/api/auth/register", userA);

        // --- register user B ---
        var userB = new RegisterRequest
        {
            Username = "userB",
            Password = "Pass123!"
        };

        await _client.PostAsJsonAsync("/api/auth/register", userB);

        // --- login user A ---
        var loginA = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Username = "userA",
            Password = "Pass123!"
        });

        var tokenA = (await loginA.Content.ReadFromJsonAsync<LoginResponse>())!.AccessToken;

        // --- create ticket as user A ---
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenA);

        var createResponse = await _client.PostAsJsonAsync("/api/tickets", new CreateTicketRequest
        {
            Title = "User A Ticket"
        });

        var createdTicket = await createResponse.Content.ReadFromJsonAsync<TicketSummaryDto>();

        var ticketId = createdTicket!.Id;

        // --- login user B ---
        var loginB = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Username = "userB",
            Password = "Pass123!"
        });

        var tokenB = (await loginB.Content.ReadFromJsonAsync<LoginResponse>())!.AccessToken;

        // --- attempt update as user B ---
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenB);

        var updateResponse = await _client.PatchAsJsonAsync(
            $"/api/tickets/{ticketId}",
            new UpdateTicketStatusRequest { Status = "Completed" });

        // --- assert forbidden ---
        Assert.Equal(HttpStatusCode.Forbidden, updateResponse.StatusCode);
    }
}