using Sandbox.Citizen;

public sealed class Mob : Component
{
    [Property]
    private float StopDistance { get; set; } = 30f;

    private NavMeshAgent Agent { get; set; }
    private SkinnedModelRenderer Target { get; set; }
    private CitizenAnimationHelper Anim { get; set; }
    private GameObject Player { get; set; }

    public enum MobStates
    {
        Chasing,
        Stunned,
        Dead,
    }

    public MobStates MobState { get; set; }

    protected override void OnAwake()
    {
        base.OnAwake();

        Agent = Components.Get<NavMeshAgent>();
        Target = Components.GetInDescendantsOrSelf<SkinnedModelRenderer>();
        Anim = Components.Get<CitizenAnimationHelper>();
        Player = Scene.Directory.FindByName("target").FirstOrDefault();
    }

    protected override void OnUpdate()
    {
        Anim.WithVelocity(Agent.Velocity);
        Anim.WithWishVelocity(Agent.WishVelocity);

        Anim.HoldType = CitizenAnimationHelper.HoldTypes.Swing;

        float distance = Vector3.DistanceBetween(Transform.Position, Player.Transform.Position);

        if (distance > StopDistance)
        {
            Agent.MoveTo(Player.Transform.Position);
        }
        else
        {
            Agent.Stop();
        }
    }
}