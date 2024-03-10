using System;
using Sandbox.Citizen;

namespace Kira;

[Category("Kira"), Icon("directions_run")]
public sealed class TPSController : Component, Component.ExecuteInEditor
{
    [Property] private Vector3 Gravity { get; set; } = new Vector3(0, 0, 800);
    [Property] public GameObject Body { get; set; }
    [Property] public GameObject Eye { get; set; }

    [Property, Group("Movement")]
    private float RunSpeed { get; set; } = 320f;
    [Property, Group("Movement")]
    private float WalkSpeed { get; set; } = 110f;


    [Property, Group("Turn"), Range(0, 100f)]
    private float MinBodyTurnVelocity { get; set; } = 10f;
    [Property, Group("Turn"), Range(0, 100f)]
    private float MinBodyTurnDifference { get; set; } = 50f;
    [Property, Group("Turn"), Range(0, 20f)]
    private float BodyTurnSpeed { get; set; } = 3f;

    [Property, Group("Camera"), Range(0, 200f)]
    private float HeightOffset { get; set; } = 75;
    [Property, Group("Camera"), Range(0, 600f)]
    private float DistanceOffset { get; set; } = 300;
    [Property, Group("Camera"), Range(0, 400)]
    private float MinDistance { get; set; } = 180f;
    [Property, Group("Camera"), Range(0, 2000)]
    private float MaxDistance { get; set; } = 620f;

    [Sync] public Angles EyeAngles { get; set; }
    [Sync] public bool IsRunning { get; set; } = true;

    private CameraComponent Cam { get; set; }
    private Vector3 WishVelocity { get; set; }
    private CitizenAnimationHelper AnimationHelper { get; set; }
    private CharacterController Controller { get; set; }

    protected override void OnEnabled()
    {
        base.OnEnabled();

        if (IsProxy)
        {
            return;
        }

        Cam = Scene.GetAllComponents<CameraComponent>().FirstOrDefault();

        if (Cam is null)
        {
            Log.Warning("Could not find a camera in the scene");
            return;
        }

        UpdateLook();

        ResetAngles();
    }

    protected override void OnAwake()
    {
        base.OnAwake();
        Controller = Components.Get<CharacterController>();
        AnimationHelper = Components.Get<CitizenAnimationHelper>();
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        Cam = Scene.GetAllComponents<CameraComponent>().FirstOrDefault();

        UpdateZoom();
        UpdateLook();
        UpdateBody();
    }

    private void UpdateZoom()
    {
        float zoomDelta = Input.MouseWheel.y;

        DistanceOffset = float.Clamp(DistanceOffset - zoomDelta * 30, MinDistance, MaxDistance);
    }

    private void UpdateLook()
    {
        if (IsProxy) return;

        var ee = EyeAngles;
        ee += Input.AnalogLook * 0.5f;
        ee.roll = 0;
        ee.pitch = float.Clamp(ee.pitch, 0, 80);
        EyeAngles = ee;

        var lookDir = EyeAngles.ToRotation();
        Cam.Transform.Position = Transform.Position + lookDir.Backward * DistanceOffset + Vector3.Up * HeightOffset;
        Cam.Transform.Rotation = lookDir;

        if (Input.Pressed("Run")) IsRunning = !IsRunning;
    }

    private void UpdateBody()
    {
        if (Body is null) return;
        if (!Controller.IsValid()) return;
        var targetAngle = Rotation.LookAt(Cam.Transform.Local.Forward.WithZ(0), Vector3.Up);
        float rotateDifference = Body.Transform.Rotation.Distance(targetAngle);


        Body.Transform.Rotation = Rotation.Lerp(Body.Transform.Rotation, targetAngle, Time.Delta * BodyTurnSpeed);
        UpdateAnimations(rotateDifference);
    }

    private void UpdateAnimations(float rotDiff = 0f)
    {
        if (AnimationHelper is null) return;

        AnimationHelper.WithVelocity(Controller.Velocity);
        AnimationHelper.WithWishVelocity(WishVelocity);
        AnimationHelper.IsGrounded = Controller.IsOnGround;
        AnimationHelper.FootShuffle = rotDiff;

        //TODO: fix
        var lookFwd = EyeAngles.Forward.EulerAngles;
        lookFwd.pitch = 0;
        AnimationHelper.WithLook(lookFwd.Forward, 0.3f, 0f, 0f);

        // AnimationHelper.IsWeaponLowered = false;
        // AnimationHelper.HoldType = CitizenAnimationHelper.HoldTypes.Rifle;
        AnimationHelper.MoveStyle = IsRunning ? CitizenAnimationHelper.MoveStyles.Run : CitizenAnimationHelper.MoveStyles.Walk;
    }

    protected override void OnFixedUpdate()
    {
        if (IsProxy) return;

        BuildWishVelocity();

        if (Controller.IsOnGround)
        {
            Controller.Velocity = Controller.Velocity.WithZ(0f);
            Controller.Accelerate(WishVelocity);
            Controller.ApplyFriction(4.0f);
        }
        else
        {
            Controller.Velocity -= Gravity * Time.Delta * 0.5f;
            Controller.Accelerate(WishVelocity.ClampLength(50));
            Controller.ApplyFriction(0.1f);
        }

        Controller.Move();

        if (!Controller.IsOnGround)
        {
            Controller.Velocity -= Gravity * Time.Delta * 0.5f;
        }
        else
        {
            Controller.Velocity = Controller.Velocity.WithZ(0);
        }
    }

    public void BuildWishVelocity()
    {
        var angles = EyeAngles;
        angles.roll = float.Clamp(angles.roll, 0, 10);
        var rot = angles.ToRotation();

        WishVelocity = rot * Input.AnalogMove;
        WishVelocity = WishVelocity.WithZ(0);

        if (!WishVelocity.IsNearZeroLength) WishVelocity = WishVelocity.Normal;
        WishVelocity *= IsRunning ? RunSpeed : WalkSpeed;
    }

    private void ResetAngles()
    {
        var ee = Cam.Transform.Rotation.Angles();
        ee.roll = 0;
        EyeAngles = ee;
    }
}