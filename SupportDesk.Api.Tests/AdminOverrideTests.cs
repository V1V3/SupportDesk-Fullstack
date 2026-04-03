using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SupportDesk.Api.Contracts;

namespace SupportDesk.Api.Tests;

public class AdminOverrideTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AdminOverrideTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Admin_Can_Update_AnotherUsers_Ticket()
    {
        var userRequest = new RegisterRequest
        {
            Username = "normalUser1",
            Password = "Pass123!"
        };

        await _client.PostAsJsonAsync("/api/auth/register", userRequest);

        var userLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Username = "normalUser1",
            Password = "Pass123!"
        });

        var userLogin = await userLoginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        var userToken = userLogin!.AccessToken;

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", userToken);

        var createResponse = await _client.PostAsJsonAsync("/api/tickets", new CreateTicketRequest
        {
            Title = "Owned by normal user"
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdTicket = await createResponse.Content.ReadFromJsonAsync<TicketSummaryDto>();
        var ticketId = createdTicket!.Id;

        var adminLoginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Username = "demo",
            Password = "Pass123!"
        });

        var adminLogin = await adminLoginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        var adminToken = adminLogin!.AccessToken;

        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", adminToken);

        var patchResponse = await _client.PatchAsJsonAsync(
            $"/api/tickets/{ticketId}",
            new UpdateTicketStatusRequest
            {
                Status = "Completed"
            });

        Assert.Equal(HttpStatusCode.NoContent, patchResponse.StatusCode);
    }
}