namespace Sandbox;

public enum SpellElementTypes
{
    Fire,
    Arcane,
    Ice,
    Earth
}

public enum SpellTypes
{
    Projectile,
    Shield
}

[GameResource("Ability Data", "ability", "Data for Abilities")]
public partial class AbilityData : GameResource
{
    public string AbilityName { get; set; }

    public float CooldownTime { get; set; } = 0.5f;
    public float WindUpTime { get; set; } = 0.1f;

    [ResourceType("image")]
    public string Icon { get; set; }

    public SpellElementTypes ElementType { get; set; }
    public SpellTypes SpellType { get; set; }
}