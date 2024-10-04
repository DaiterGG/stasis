using System;
using Stasis.Player;

namespace Stasis.Zones;
public sealed class EndZone : Component, Component.ITriggerListener, IZone
{
    public int ID { get; set; } = 1;
    public int Locked { get; set; } = 0;
    public void OnTriggerEnter(Collider col)
    {
        if (col.GameObject.Tags.Contains("particle") &&
            !Sng.Inst.Player.SpinC.IsAttached) return;
        var eng = col.GameObject.Components.Get<EngineComponent>();
        if (col is BoxCollider && eng != null) return;
        Sng.Inst.ZoneC.EndZoneEnter(GameObject, col);
    }
}
