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
        // Sng.ELog("list: " + GameObject.Name);
        foreach (var blade in GameObject.Children.ToList())
        {
            // Sng.ELog("blade: " + blade);
            var trigger = blade.Components.Get<SpinTrigger>();
            if (blade.Enabled && trigger != null)
            {
                blades.Add(trigger);
                RestartAllBlades += trigger.ResetPos;
                trigger.OnAwakeInit();
            }
        }
        // Sng.ELog(RestartAllBlades);
        RestartSpin();
    }
    public void OnFixedGlobal()
    {
        foreach (var blade in blades)
        {
            blade.OnFixedGlobal();
        }
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
        IsAttached = false;
        foreach (var x in blades)
        {
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

            x.GameObject.SetParent(PLAYEROBJ, false);
        }
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
