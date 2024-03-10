namespace Kira;

public abstract class BaseAbility
{
    public string AbiltiyName { get; set; }
    public float CooldownTime { get; set; }
    public float WindUpTime { get; set; }
    public string Icon { get; set; }

    public abstract void CastSpell();
}