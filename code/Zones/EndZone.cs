using System;
using Stasis.Player;

namespace Stasis.Zones;
public sealed class EndZone : Component, Component.ITriggerListener, IZone
{
    public int ID { get; set; } = 1;

    //locked by default
    public int Locked { get; set; } = 0;

    public void OnTriggerEnter(Collider col)
    {
        //player component check
        if (!col.GameObject.Tags.Contains("player")) return;

        //break propeller blades check
        if (col.GameObject.Tags.Contains("particle") &&
            !Sng.Inst.Player.SpinC.IsAttached) return;

        //new big box(tm) check
        if (col.GameObject.Tags.Contains("ghost")) return;

        Sng.Inst.ZoneC.EndZoneEnter(GameObject, col);
    }
}
