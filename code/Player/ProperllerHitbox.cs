namespace Stasis.Player;

public sealed class ProperllerHitbox : Component, Component.ICollisionListener
{
    public void OnCollisionStart(Collision col)
    {
        Log.Info("Collided with: " + col.Other.GameObject.Name);
        if (!Sng.Inst.Player.SpinC.IsAttached) return;
        if (col.Other.GameObject.Tags.Contains("particle")) return; // not secessary since blades have 'player' tag
        Sng.Inst.Player.SpinC.SpinCollision();
    }
}
