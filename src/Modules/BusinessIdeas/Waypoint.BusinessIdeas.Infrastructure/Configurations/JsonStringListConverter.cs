using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Waypoint.BusinessIdeas.Infrastructure.Configurations;

/// <summary>
/// Maps a List&lt;string&gt; property onto a Postgres jsonb column. Npgsql has native jsonb
/// support, but a plain serialize/deserialize converter is the simplest thing that works
/// reliably across the versions pinned in this project, and it keeps the mapping explicit rather
/// than depending on runtime type discovery.
/// </summary>
internal static class JsonStringListConverter
{
    public static readonly ValueConverter<List<string>, string> Converter = new(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
        v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

    public static readonly ValueComparer<List<string>> Comparer = new(
        (a, b) => (a ?? new List<string>()).SequenceEqual(b ?? new List<string>()),
        v => (v ?? new List<string>()).Aggregate(0, (hash, s) => HashCode.Combine(hash, s.GetHashCode())),
        v => (v ?? new List<string>()).ToList());
}
