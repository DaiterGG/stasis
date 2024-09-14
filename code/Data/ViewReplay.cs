using System;
using Stasis.Player;

namespace Stasis.Data;

public class ViewReplay
{
	EngineComponent ENGINE;
	Replay Replay;
	public float CurrentTick { get; private set; }
	public int OldestTick { get; private set; }
	public bool PropellerBroken { get; private set; }
	public float TimerStart { get; private set; }
	public bool IsPlaying { get; private set; }
	public float ReplaySpeed { get; private set; } = 1.0f;
	public void OnAwakeInit()
	{
		ENGINE = Sng.Inst.Player.Engine;
	}

	public void OnUpdateGlobal()
	{
		if ( !IsPlaying ) return;
		var newTick = CurrentTick + (Time.Delta / ReplaySpeed);
		int previousTick = ReplaySpeed >= 0 ? (int)newTick : newTick.CeilToInt();
		int nextTick = ReplaySpeed >= 0 ? (int)newTick : newTick.CeilToInt();
		float factor = newTick - previousTick;

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
		CurrentTick = newTick;

		if ( Math.Abs( (float)OldestTick - CurrentTick ) > 1 )
		{
			OldestTick = (int)CurrentTick;
			if ( Replay.Actions.ContainsKey(OldestTick) )
			{
				ApplyActions( Replay.Actions[OldestTick] );
			}

		}
	}

	void ApplyActions( List<Action> actions )
	{
		foreach ( var action in actions )
		{
			if ( action == Action.TimerStart )
			{

			}
			else if ( action == Action.PropellerRepair )
			{

			}
			else if ( action == Action.PropellerBreak )
			{

			}
		}
	}

	public void JumpToTick( int tick )
	{
		CurrentTick = tick;
		OldestTick = tick;
		IsPlaying = true;
		for ( int i = 0; i <= tick; i++ ){
			if ( Replay.Actions.ContainsKey(i) ){
				ApplyActions( Replay.Actions[i] );
			}
		}
	}
}
