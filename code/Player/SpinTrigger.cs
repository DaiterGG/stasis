using System;

namespace Stasis.Player;

public sealed class SpinTrigger : Component, Component.ICollisionListener
{
    [Property, Range(0, 360f)] public float rotationOffset;
    SpinControl SPIN;
    public Rigidbody rig;

    public void OnAwakeInit()
    {
        rig = GameObject.Components.Get<Rigidbody>();
        SPIN = Sng.Inst.Player.SpinC;
        ResetPos();

    }

    public void OnCollisionStart(Collision col)
    {
        if (!SPIN.IsAttached) return;
        if (col.Other.GameObject.Tags.Contains("particle")) return; // not secessary since blades have 'player' tag
        SPIN.SpinCollision();
    }
    public void OnFixedGlobal()
    {
        if (!SPIN.IsAttached)
        {
            rig.ApplyForce(new Vector3(0, 0, SPIN.BladeGravity * -1));
            return;
        }
        else if (Math.Abs(LocalPosition.y) > 4f
            || Math.Abs(LocalRotation.Pitch()) > 1f
            || Math.Abs(LocalRotation.Roll()) > 1f
            || LocalPosition.z != 0)
        {
            ResetPos();
        }
    }
    public void ResetPos()
    {
        try
        {
            GameObject.SetParent(SPIN.PropRig.GameObject, false);
        }
        catch { }

        WorldPosition = SPIN.PropRig.WorldPosition;
        WorldRotation = SPIN.PropRig.WorldRotation
            * Rotation.From(0, rotationOffset, 0);
        WorldPosition += new Vector3(0, -1f, 0) * WorldRotation;
        Transform.ClearInterpolation();
        rig.Velocity = 0;
        rig.AngularVelocity = 0;
    }
}
