namespace Waypoint.Dreams.Domain;

public sealed class DreamStatement
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid DreamId { get; init; }
    public string Statement { get; set; } = string.Empty;
    public string? Purpose { get; set; }
    public string? WhoItHelps { get; set; }
    public string? Problem { get; set; }
    public string? Outcome { get; set; }
    public string? Motivation { get; set; }
    public string? Impact { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public static DreamStatement Create(Guid dreamId, string statement, string? purpose, string? whoItHelps,
        string? problem, string? outcome, string? motivation, string? impact) =>
        new()
        {
            DreamId = dreamId,
            Statement = statement,
            Purpose = purpose,
            WhoItHelps = whoItHelps,
            Problem = problem,
            Outcome = outcome,
            Motivation = motivation,
            Impact = impact,
        };
}
