
using Stasis.Player;

namespace Stasis.Zones;

public sealed class Teleport : Component, Component.ITriggerListener, IZone
{
    public int ID { get; set; }
    public int Locked { get; set; } = 0;
    [Property]
    public bool ResetRotation = true;
    [Property]
    public bool ResetVelocity = true;
    [Property]
    public bool RepairPropeller = true;
    [Property, Description("Could be any game object")]
    public GameObject Exit;
    protected override void OnStart()
    {
        base.OnStart();
        if (Exit == null)
        {
            Log.Warning("Teleport with no spawn");
            Exit = GameObject;
        }
    }
    public void OnTriggerEnter(Collider col)
    {
        Log.Info("Collided with: " + col.GameObject.Name);
        //player component check
        if (!col.GameObject.Tags.Contains("player")) return;

        //break propeller blades check
        if (col.GameObject.Tags.Contains("particle") &&
            !Sng.Inst.Player.SpinC.IsAttached) return;

        //old big box(tm) check
        //var eng = col.GameObject.Components.Get<EngineComponent>();
        //if (col is BoxCollider && eng != null) return;

        //new big box(tm) check
        if (col.GameObject.Tags.Contains("ghost")) return;

        //teleport is locked check
        if (Locked != 0) Sng.Inst.ZoneC.ZoneBlockedMessage();
        //else col.WorldPosition = Exit.WorldTransform.Position;
        else Sng.Inst.ZoneC.TeleportPlayer(Exit.WorldTransform, ResetVelocity, ResetRotation, RepairPropeller);
    }

    public void OnTriggerExit(Collider collider)
    {
        Log.Info("Ended colliding with: " + collider.GameObject.Name);
    }
}
