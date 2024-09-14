using System;
using Stasis.Player;

namespace Stasis.Data;

public class RecordReplay
{
	EngineComponent ENGINE;
	Replay Replay;
	List<Action> CurrentActions;
	bool IsRecording { get; set; }
	public void OnAwakeInit()
	{
		ENGINE = Sng.Inst.Player.Engine;
	}

	public void OnFixedGlobal()
	{
		if ( !IsRecording ) return;
		Replay.Ticks.Add( new Tick(
			ENGINE.Transform.Position,
			(short)(ENGINE.Transform.Rotation.Pitch() * 100),
			(short)(ENGINE.Transform.Rotation.Yaw() * 100),
			(short)(ENGINE.Transform.Rotation.Roll() * 100)
		) );
		
		if ( CurrentActions.Count > 0 ){
			Replay.Actions[Replay.Ticks.Count - 1] = CurrentActions;
			CurrentActions = new List<Action>();
		}
	}
	public void Start(){
		Replay = new Replay();
		CurrentActions = new List<Action>();
		IsRecording = true;
	}

	public void Pause(){
		IsRecording = false;
	}
	
	public static void ActionHappened( Action action ){
		var self = Sng.Inst.RecordC;
		if ( !self.IsRecording ) return;
		self.CurrentActions.Add( action );
	}
}
