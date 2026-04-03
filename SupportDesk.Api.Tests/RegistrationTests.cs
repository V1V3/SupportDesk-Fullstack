using System.Net;
using System.Net.Http.Json;
using SupportDesk.Api.Contracts;

namespace SupportDesk.Api.Tests;

public class RegistrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RegistrationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithDuplicateUsername_ReturnsConflict()
    {
        var request = new RegisterRequest
        {
            Username = "duplicateUser",
            Password = "Pass123!"
        };

        var firstResponse = await _client.PostAsJsonAsync("/api/auth/register", request);
        var secondResponse = await _client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }
}