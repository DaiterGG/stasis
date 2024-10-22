namespace Stasis.Player;

public sealed class JumpComponent : Component, Component.ICollisionListener
{
    int collisions = 0;
    bool active = false;
    public void OnCollisionStart(Collision col)
    {
        if (col.Self.GameObject.Name == "Body")
        {
            if ((col.Contact.Point - col.Self.GameObject.WorldPosition).z > -2)
                collisions = 20;
        }
    }
    protected override void OnFixedUpdate()
    {
        active = collisions > 0;
        collisions--;
    }
    public void TryToJump()
    {
        if (active && !Sng.Inst.Player.SpinC.IsAttached)
        {
            if (!Sng.Inst.Player.Engine.isRunning)
            {
                Sng.Inst.Player.Engine.EngOn(true);

            }
            Sng.Inst.Player.Engine.rigid.ApplyImpulse(Sng.Inst.Player.Body.WorldRotation
            * new Vector3(200, 0, 300));
            collisions = 0;
        }
    }
}
