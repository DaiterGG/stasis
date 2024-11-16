using Stasis.Player;

namespace Stasis.Data;

public class ViewReplay
{
    EngineComponent ENGINE;
    ReplayUI UI;
    Sng SNG;
    SpinControl SPIN;
    public GameObject CallbackUI;
    public Replay Replay { get; private set; }
    public float CurrentTick { get; set; }
    public int OldestTick { get; private set; }
    public bool PropellerBroken { get; private set; }
    public float TimerStart { get; private set; }
    public bool IsPlaying { get; private set; }
    public float ReplaySpeed { get; set; } = 1.0f;
    const float SECONDS_TO_TICKS = 50f;
    public void OnAwakeInit()
    {
        SNG = Sng.Inst;
        ENGINE = SNG.Player.Engine;
        UI = SNG.MenuC.ReplayUI;
        SPIN = SNG.Player.SpinC;
        PauseView();
    }
    public void PauseView( bool pause = true )
    {
        IsPlaying = !pause;
    }

    public void Watch( Replay rep, GameObject UIThatCalled )
    {
        if ( rep.Ticks.Count == 0 ) rep.Ticks = ReplaySerialize.FromStrToTicks( rep.TicksUTF );
        Replay = rep;
        SNG.ChangeGameState( GameState.MainMenu );
        SNG.ChangeGameState( GameState.ViewReplay );
        PauseView();
        JumpToTick( 0 );
        CallbackUI = UIThatCalled;
    }

    public void OnUpdateGlobal()
    {
        if ( Input.Pressed( "Test1" ) )
        {
            if ( UI.GameObject.Enabled == true )
                Sng.Inst.ChangeGameState( GameState.MainMenu );
            else
                Sng.Inst.ChangeGameState( GameState.ViewReplay );
        }

        if ( !IsPlaying || Replay == null ) return;
        NextTick();
    }
    void NextTick()
    {
        var newTick = CurrentTick + (Time.Delta * SECONDS_TO_TICKS * ReplaySpeed);
        if ( newTick > Replay.Ticks.Count - 1 )
        {
            JumpToTick( 0 );
            return;
        }
        if ( newTick < 0 )
        {
            JumpToTick( Replay.Ticks.Count - 1 );
            return;
        }
        int previousTick = ReplaySpeed >= 0 ? (int)CurrentTick : (int)Math.Ceiling( CurrentTick );
        int nextTick = ReplaySpeed >= 0 ? (int)Math.Ceiling( newTick ) : (int)newTick;
        float factor = (newTick - previousTick) / (nextTick - previousTick);

        //Log.Info(newTick + " " + previousTick + " " + nextTick + " " + CurrentTick);

        var prevRotation = Rotation.From(
            Replay.Ticks[previousTick].Pitch / 100f,
            Replay.Ticks[previousTick].Yaw / 100f,
            Replay.Ticks[previousTick].Roll / 100f
        );
        var nextRotation = Rotation.From(
            Replay.Ticks[nextTick].Pitch / 100f,
            Replay.Ticks[nextTick].Yaw / 100f,
            Replay.Ticks[nextTick].Roll / 100f
        );
        ENGINE.ApplyTick(
            Vector3.Lerp( Replay.Ticks[previousTick].Transform, Replay.Ticks[nextTick].Transform, factor ),
            Rotation.Lerp( prevRotation, nextRotation, factor ) );
        // Log.Info(" CurrentTick " + CurrentTick + " NewTick " + newTick +" PreviousTick " + previousTick +" NextTick " + nextTick +" Factor " + factor);
        // Log.Info( " Pitch " + Replay.Ticks[nextTick].Pitch + " Yaw " + Replay.Ticks[nextTick].Yaw + " Roll " + Replay.Ticks[nextTick].Roll );
        CurrentTick = newTick;

        var ticksPassed = OldestTick - CurrentTick;
        if ( ticksPassed >= 1 || ticksPassed <= -1 )
        {
            for ( int i = 1; i <= Math.Abs( ticksPassed ); i++ )
            {
                var next = OldestTick + i * Math.Sign( ticksPassed );

                if ( Replay.Actions.ContainsKey( next ) )
                {
                    ApplyActions( Replay.Actions[next], ReplaySpeed >= 0, false );
                }
                SPIN.ApplySpinSpeed( next );
            }
            OldestTick = (int)CurrentTick;
        }
        UI.TickRelay = (int)CurrentTick;
    }
    /// <param name="viewForward">time direction</param>
    /// <param name="jumpTo">jump to tick or go from previous tick</param>
    void ApplyActions( List<Action> actions, bool viewForward, bool jumpTo )
    {
        foreach ( var action in actions )
        {
            if ( action == Action.PropellerRepair || action == Action.PropellerBreak )
                ApplyPropellerAction( action, viewForward, jumpTo );
        }
    }
    void ApplyPropellerAction( Action propellerAction, bool viewForward, bool jumpTo )
    {

        if ( propellerAction == Action.PropellerRepair )
        {
            if ( viewForward )
                SPIN.RestartSpin();
            else
                SPIN.BreakSpin( !jumpTo, Replay.Ticks[OldestTick].Spin );
        }
        else if ( propellerAction == Action.PropellerBreak )
        {
            if ( viewForward )
                SPIN.BreakSpin( !jumpTo, Replay.Ticks[OldestTick].Spin );
            else
                SPIN.RestartSpin();
        }
    }

    public void JumpToTick( int tick )
    {
        CurrentTick = tick;
        OldestTick = tick;
        ENGINE.ApplyTick( Replay.Ticks[tick].Transform,
            Rotation.From( Replay.Ticks[tick].Pitch / 100f, Replay.Ticks[tick].Yaw / 100f, Replay.Ticks[tick].Roll / 100f ) );
        for ( int i = 0; i <= tick; i++ )
        {
            if ( Replay.Actions.ContainsKey( i ) )
            {
                ApplyActions( Replay.Actions[i], true, true );
            }
        }
    }
}
