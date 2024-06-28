namespace Movies.Contracts.Responses;

public class ValidationFaliureResponse
{
    public required IEnumerable<ValidationResponse> Errors { get; init; }
}

public class ValidationResponse
{
    public required string ProperyName { get; init; }
    public required string Message { get; init; }
}
