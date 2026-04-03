namespace SupportDesk.Api.Contracts;

public class CurrentUserResponse
{
    public string? Username { get; set; }
    public string? Role { get; set; }
    public List<ClaimResponse> Claims { get; set; } = new();
}

public class ClaimResponse
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}