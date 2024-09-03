using System.Numerics;
using System;
using Stasis.UI;
namespace Stasis.Data;

public struct SaveState
{
	public readonly Vector3 Transform;
	public readonly Rotation Rotation;
	public readonly Vector3 Velocity;
	public readonly Vector3 AngularVelocity;
	public readonly float Time;
	public readonly bool SpinAttached;
	public readonly bool EngineRunning;
	public readonly float Gain;
	public readonly int Progress;
	public readonly int[] ActiveBeacons;
	
	public SaveState(Vector3 tr, Rotation rt, Vector3 v, Vector3 av, float t, bool sb, bool er, float g, int p, int[] ab){
		Transform = tr;
		Rotation = rt;
		Velocity = v;
		AngularVelocity = av;
		Time = t;
		SpinAttached = sb;
		EngineRunning = er;
		Gain = g;
		Progress = p;
		ActiveBeacons = ab;
	}
}
 public class SaveStateControl
 {
	Timer TIMER;
	GameObject BODY;
	MenuController MENUC;
	public bool Enabled {get; set;}
	public int SelectedId {get; private set;}
	public List<SaveState> SaveStates;
	public float CurrentTime {get; private set;}
	public bool IsRunning {get; private set;}
	/// <summary>
	/// Called once a Game
	/// </summary>
	public void OnAwakeInit(){
		TIMER = Sng.Inst.Timer;
		BODY = Sng.Inst.Player.Body;
		MENUC = Sng.Inst.MenuC;
		InitSaveStates();
	}
	/// <summary>
	/// Called once a Map
	/// </summary>
	public void InitSaveStates(){
		SaveStates = new List<SaveState>();
		Enabled = false;
		SelectedId = -1;
		Reset();
	}
	/// <summary>
	/// Called once a Reset pressed
	/// </summary>
	public void Reset(){
		IsRunning = false;
		CurrentTime = 0;
	}
	public void OnUpdateGlobal(){
		if(IsRunning) CurrentTime += Time.Delta;	
	}
	public void OnFixedGlobal(){
		if (Input.Pressed("Toggle")){
			Enabled = !Enabled;
		}
		if (!Enabled) return;
		if (Input.Pressed("Load")) {
			Load();
		}
		if (Input.Pressed("Save")) {
			Add();
		}
		if (Input.Pressed("Next")) {
			Move(true);
		}
		if (Input.Pressed("Prev")) {
			Move(false);
		}
		if (Input.Pressed("First")) {
			ToStart(true);
		}
		if (Input.Pressed("Last")) {
			ToStart(false);
		}
		if (Input.Pressed("Remove")) {
			Remove();	
		}
		if (Input.Pressed("Wipe")) {

			Remove(true);	
		}
	}

	public void Add(){
		var r = BODY.Components.Get<Rigidbody>();
		float time;
		if ( IsRunning ) time = CurrentTime;
		else time = TIMER.timerSeconds;
		SaveStates.Add(GetSaveState( time ) );
		SelectedId = SaveStates.Count - 1;
	}
	public void Load(){
		if (SelectedId == -1) return;
		TIMER.StopTimer();
		var state = SaveStates[SelectedId];

		ApplySaveState(state);
		IsRunning = state.Time != 0;
		CurrentTime = state.Time;
		MENUC.EndUI.GameObject.Enabled = false;
	}
	public void Remove(bool all = false){
		if (SelectedId == -1) return;
		if(all) 
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
	public void Move(bool forward){
		if (SelectedId == -1) return;
		if( forward )  SelectedId++; 
		else SelectedId--;
		SelectedId = Math.Max(0,Math.Min(SaveStates.Count - 1,SelectedId));
	}
	public void ToStart(bool toStart){
		if (SelectedId == -1) return;
		SelectedId = toStart ? SaveStates.Count - 1 : 0;
	}

	static void ApplySaveState( SaveState state ){
		var eng = Sng.Inst.Player.Engine;
		eng.ApplySaveState(state);
		var spin = Sng.Inst.Player.SpinC;
		spin.ApplySaveState(state);
		var zone = Sng.Inst.ZoneC;
		zone.ApplySaveState(state);
	}
	static SaveState GetSaveState( float time ){
		var eng = Sng.Inst.Player.Engine;
		var spin = Sng.Inst.Player.SpinC;
		var zone = Sng.Inst.ZoneC;
		return new SaveState(
			eng.Transform.Position,
			eng.Transform.Rotation,
			eng.rigid.Velocity,
			eng.rigid.AngularVelocity,
			time,
			spin.isAttached,
			eng.isRunning,
			eng.gain,
  			eng.progress,
			zone.GetActiveBeacons()
		);
	}

 }
