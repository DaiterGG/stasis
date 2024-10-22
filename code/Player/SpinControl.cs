using System;
using Stasis.Data;

namespace Stasis.Player;

public sealed class SpinControl : Component
{
    public Rigidbody PropRig;
    [Property] public BoxCollider BigBodyBox;
    [Property, Range(0, 10000f, 100f)] public float BladeGravity { get; set; } = 1500f;
    EngineComponent ENGINE;
    GameObject PLAYEROBJ;
    Timer TIMER;
    float speedMult = 0.2f;
    public bool IsAttached { get; private set; }
    List<SpinTrigger> blades = new List<SpinTrigger>();
    public void OnAwakeInit()
    {
        ENGINE = Sng.Inst.Player.Engine;
        PLAYEROBJ = Sng.Inst.Player.GameObject;
        TIMER = Sng.Inst.Timer;
        PropRig = GameObject.Components.Get<Rigidbody>();
        var l = GameObject.Children.ToList();
        for (var i = 0; i < l.Count; i++)
        {
            var t = l[i].Components.Get<SpinTrigger>();
            if (l[i].Enabled && t != null)
            {
                blades.Add(t);
                RestartAllBlades += t.ResetPos;
                t.OnAwakeInit();
            }
        }
        RestartSpin();
    }
    public void OnFixedGlobal()
    {
        for (int i = 0; i < blades.Count; i++)
        {
            blades[i].OnFixedGlobal();
        }
        /*foreach (var blade in blades)*/
        /*{*/
        /*    blade.OnFixedGlobal();*/
        /*}*/
        if (!IsAttached) return;
        ApplySpinSpeed(ENGINE.progress == 100 ? (int)(ENGINE.gain / ENGINE.maxGain * 100f) : ENGINE.progress);
    }
    /// <summary>
    /// Called when blade physically collide
    /// </summary>
    public void SpinCollision()
    {
        if (!IsAttached || !ENGINE.IsGaming) return;

        BreakSpin(true, ENGINE.progress == 100 ? (int)(ENGINE.gain / ENGINE.maxGain * 100f) : ENGINE.progress);
    }

    /// <param name="bladesExplode">true - explode, false - disapear</param>
    /// <param name="brakeForce">0-100</param>
    public void BreakSpin(bool bladesExplode, int brakeForce = 0)
    {
        if (IsAttached) RecordReplay.ActionHappened(Data.Action.PropellerBreak);
        for (int i = 0; i < blades.Count; i++)
        {
            var x = blades[i];
            if (bladesExplode)
            {
                x.WorldPosition = WorldPosition;
                var rig = x.Components.Get<Rigidbody>();
                rig.ApplyImpulse(new Vector3(0, 800f * (brakeForce + 5f) / 100f, 100f) * x.WorldRotation);
                rig.ApplyTorque(new Vector3(4000f));
            }
            else
            {
                x.WorldPosition = new Vector3(9999999, 9999999, 9999999);
            }
            x.Transform.ClearInterpolation();

            x.GameObject.Parent = PLAYEROBJ;
        }
        IsAttached = false;
        BigBodyBox.Scale = new Vector3(0);
        TIMER.Update();
    }
    delegate void ResetAll();
    ResetAll RestartAllBlades;
    public void RestartSpin()
    {
        if (!IsAttached)
            RecordReplay.ActionHappened(Data.Action.PropellerRepair);
        IsAttached = true;
        if (RestartAllBlades == null) Log.Error("blades init error");

        RestartAllBlades();

        BigBodyBox.Scale = new Vector3(250);
        TIMER.Update();
    }

    public void ApplySaveState(SaveState state)
    {
        if (state.SpinAttached)
        {
            if (!IsAttached) RestartSpin();
            return;
        }
        BreakSpin(false);
    }
    public void ApplySpinSpeed(int speed)
    {
        WorldRotation *= Rotation.From(0, speed * speedMult, 0);
    }
}
