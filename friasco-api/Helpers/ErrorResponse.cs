namespace friasco_api.Helpers;

public class ErrorResponse
{
    public string Type { get; set; }
    public string Title { get; set; }
    public int Status { get; set; }
    public string TraceId { get; set; }
    public Dictionary<string, string[]> Errors { get; }

    public ErrorResponse(string type = "", string title = "", int status = 0, string traceId = "")
    {
        Type = type;
        Title = title;
        Status = status;
        TraceId = traceId;
        Errors = new Dictionary<string, string[]>();
    }
}
