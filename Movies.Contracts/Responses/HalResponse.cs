using System.Text.Json.Serialization;

namespace Movies.Contracts.Responses;

//hal - hypermedia api language
public abstract class HalResponse
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<Link> Links { get; set; } = new List<Link>(); 
}

public class Link
{
    public required string Href { get; init; }
    public required string Rel { get; init; }
    public required string Type { get; init; }
}
