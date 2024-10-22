using System.Numerics;
using System;
using Stasis.UI;
using Stasis.Player;
namespace Stasis.Data;

public struct SaveState
{
    public Vector3 Transform;
    public Rotation Rotation;
    public Vector3 Velocity;
    public Vector3 AngularVelocity;
    public float Time;
    public bool SpinAttached;
    public bool EngineRunning;
    public float Gain;
    public int Progress;
    public int[] ActiveBeacons;
    public int? CheckPointActivated;

}
public class SaveStateControl
{
    Timer TIMER;
    EngineComponent ENGINE;
    MenuController MENUC;
    public bool Enabled { get; set; }
    public int SelectedId { get; private set; }
    public List<SaveState> SaveStates;
    public float CurrentTime { get; private set; }
    public bool IsRunning { get; private set; }
    /// <summary>
    /// Called once a Game
    /// </summary>
    public void OnAwakeInit()
    {
        TIMER = Sng.Inst.Timer;
        ENGINE = Sng.Inst.Player.Engine;
        MENUC = Sng.Inst.MenuC;
        InitSaveStates();
    }
    /// <summary>
    /// Called once a Map
    /// </summary>
    public void InitSaveStates()
    {
        SaveStates = new List<SaveState>();
        Enabled = false;
        SelectedId = -1;
        Reset();
    }
    /// <summary>
    /// Called once a Reset pressed
    /// </summary>
    public void Reset()
    {
        IsRunning = false;
        CurrentTime = 0;
    }
    public void OnUpdateGlobal()
    {
        if (IsRunning) CurrentTime += Time.Delta;
    }
    public void OnFixedGlobal()
    {
        if (Input.Pressed("Toggle"))
        {
            Enabled = !Enabled;
        }
        if (!Enabled) return;
        if (Input.Pressed("Load"))
        {
            Load();
        }
        if (Input.Pressed("Save"))
        {
            Add();
        }
        if (Input.Pressed("Next"))
        {
            Move(true);
        }
        if (Input.Pressed("Prev"))
        {
            Move(false);
        }
        if (Input.Pressed("First"))
        {
            ToStart(true);
        }
        if (Input.Pressed("Last"))
        {
            ToStart(false);
        }
        if (Input.Pressed("Remove"))
        {
            Remove();
        }
        if (Input.Pressed("Wipe"))
        {

            Remove(true);
        }
    }

    public void Add()
    {
        float time;
        if (IsRunning) time = CurrentTime;
        else time = TIMER.timerSeconds;
        SaveStates.Add(GetSaveState(time));
        SelectedId = SaveStates.Count - 1;
    }
    public void Load()
    {
        if (SelectedId == -1) return;
        TIMER.StopTimer();
        var state = SaveStates[SelectedId];

        ApplySaveState(state);
        IsRunning = state.Time != 0;
        CurrentTime = state.Time;
        MENUC.EndUI.GameObject.Enabled = false;
    }
    public void Remove(bool all = false)
    {
        if (SelectedId == -1) return;
        if (all)
        {
            SaveStates.Clear();
            SelectedId = -1;
        }
        else
        {
            SaveStates.RemoveAt(SelectedId);
            SelectedId--;
        }
    }
    public void Move(bool forward)
    {
        if (SelectedId == -1) return;
        if (forward) SelectedId++;
        else SelectedId--;
        SelectedId = Math.Max(0, Math.Min(SaveStates.Count - 1, SelectedId));
    }
    public void ToStart(bool toStart)
    {
        if (SelectedId == -1) return;
        SelectedId = toStart ? SaveStates.Count - 1 : 0;
    }

    public static void ApplySaveState(SaveState state)
    {
        var eng = Sng.Inst.Player.Engine;
        eng.ApplySaveState(state);

        var spin = Sng.Inst.Player.SpinC;
        spin.ApplySaveState(state);

        var zone = Sng.Inst.ZoneC;
        zone.ApplySaveState(state);
    }
    public static SaveState GetSaveState(float time)
    {
        var eng = Sng.Inst.Player.Engine;
        var spin = Sng.Inst.Player.SpinC;
        var zone = Sng.Inst.ZoneC;
        return new SaveState()
        {
            Transform = eng.WorldPosition,
            Rotation = eng.WorldRotation,
            Velocity = eng.rigid.Velocity,
            AngularVelocity = eng.rigid.AngularVelocity,
            Time = time,
            SpinAttached = spin.IsAttached,
            EngineRunning = eng.isRunning,
            Gain = eng.gain,
            Progress = eng.progress,
            ActiveBeacons = zone.GetActiveBeacons(),
            CheckPointActivated = zone.CheckPointActivated
        };
    }

}
