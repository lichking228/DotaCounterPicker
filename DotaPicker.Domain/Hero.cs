using System.Text.Json.Serialization;

namespace DotaPicker.Domain;

public record Hero(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("localized_name")] string Name,
    [property: JsonPropertyName("primary_attr")] string PrimaryAttr,
    [property: JsonPropertyName("attack_type")] string AttackType,
    [property: JsonPropertyName("roles")] string[] Roles
);