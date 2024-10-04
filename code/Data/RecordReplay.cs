using Stasis.Player;

namespace Stasis.Data;

public class RecordReplay
{
    EngineComponent ENGINE;
    SpinControl SPIN;
    public Replay Replay;
    List<Action> CurrentActions;
    bool IsRecording { get; set; }
    public void OnAwakeInit()
    {
        ENGINE = Sng.Inst.Player.Engine;
        SPIN = Sng.Inst.Player.SpinC;
        Pause();
    }

    public void OnFixedGlobal()
    {
        /*
        if (Input.Pressed("Attack2"))
        {
            if (IsRecording)
                Pause();
            else
                Start();
        }
        */
        if (!IsRecording) return;
        Replay.Ticks.Add(new Tick(
            ENGINE.Transform.Position,
            (short)(ENGINE.Transform.Rotation.Pitch() * 100),
            (short)(ENGINE.Transform.Rotation.Yaw() * 100),
            (short)(ENGINE.Transform.Rotation.Roll() * 100),
            ENGINE.progress == 100 ? (byte)(ENGINE.gain / ENGINE.maxGain * 100) : (byte)ENGINE.progress
        ));

        if (CurrentActions.Count > 0)
        {
            Replay.Actions[Replay.Ticks.Count - 1] = CurrentActions;
            CurrentActions = new List<Action>();
        }
    }
    public void Start()
    {
        Sng.ELog("replay reStarted");
        Replay = new Replay();
        CurrentActions = new List<Action>();
        IsRecording = true;
        collectInitialState();

        // NOTE: subject to change
        TimerStart();
    }
    public void collectInitialState()
    {
        // TODO: if timer running calc start time from it
        ActionHappened(SPIN.IsAttached ? Action.PropellerRepair : Action.PropellerBreak);
    }
    public Replay? TryToGet()
    {
        if (IsRecording)
        {
            Pause();
            Replay.TicksUTF = ReplaySerialize.TicksToStr(Replay.Ticks);
            return Replay;
        }
        else
        {
            return null;
        }
    }
    public void Pause()
    {
        IsRecording = false;
        //
        /*
        if ( Replay == null ) return;
        foreach ( var tick in Replay.Ticks )
        {
            // Sng.ELog( tick.Transform + " " + tick.Yaw + " " + tick.Pitch + " " + tick.Roll );
        }
        foreach ( var actions in Replay.Actions )
        {
            Sng.ELog( actions.Key + " " +actions.Value.First());
        }
        */
    }

    public static void ActionHappened(Action action)
    {
        var self = Sng.Inst.RecordC;
        if (!self.IsRecording) return;
        self.CurrentActions.Add(action);
    }
    public static void TimerStart()
    {
        var self = Sng.Inst.RecordC;
        if (!self.IsRecording) return;
        self.Replay.StartTime = self.Replay.Ticks.Count;
    }
    public static void TimerStop()
    {
        var self = Sng.Inst.RecordC;
        if (!self.IsRecording) return;
        self.Replay.EndTime = self.Replay.Ticks.Count;
    }
}
