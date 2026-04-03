namespace SupportDesk.Api.Contracts;

public class GetTicketsQuery
{
    public string? Status { get; set; }

    public string? Search { get; set; }

    public string? CreatedBy { get; set; }

    public string? SortBy { get; set; } = "createdAtUtc";

    public string? SortDirection { get; set; } = "desc";

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}