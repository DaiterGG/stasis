using System;
using Stasis.Data;

namespace Stasis.Player;

public sealed class SpinControl : Component
{
	[Property] public Rigidbody PropRig;
	[Property, Range( 0, 10000f, 100f )] public float BladeGravity { get; set; } = 1500f;
	EngineComponent ENGINE;
	GameObject PLAYEROBJ;
	Timer TIMER;
	BoxCollider BigBodyBox;
	float speedMult = 0.2f;
	public bool IsAttached { get; private set; }
	List<SpinTrigger> blades = new List<SpinTrigger>();
	public void OnAwakeInit()
	{
		ENGINE = Sng.Inst.Player.Engine;
		PLAYEROBJ = Sng.Inst.Player.GameObject;
		TIMER = Sng.Inst.Timer;
		BigBodyBox = ENGINE.GameObject.Components.Get<BoxCollider>();
		PropRig.GameObject.Children.ToList().ForEach( x =>
		{
			var t = x.Components.Get<SpinTrigger>();
			if ( x.Enabled && t != null )
			{
				blades.Add( t );
				RestartAllBlades += t.ResetPos;
				t.OnAwakeInit();
			}
		} );
		RestartSpin();
	}
	public void OnFixedGlobal()
	{
		foreach ( var blade in blades )
		{
			blade.OnFixedGlobal();
		}
		if ( !IsAttached ) return;
		if ( ENGINE.isRunning )
		{
			PropRig.Transform.Rotation *= Rotation.From( 0, ENGINE.gain / ENGINE.gravity * 100 * speedMult, 0 );
		}
		else PropRig.Transform.Rotation *= Rotation.From( 0, ENGINE.progress * speedMult, 0 );

	}
	public void SpinCollision()
	{
		if ( !IsAttached ) return;

		blades.ForEach( x =>
		{
			x.GameObject.Parent = PLAYEROBJ;
			x.Transform.Position = PropRig.Transform.Position;
			var rig = x.Components.Get<Rigidbody>();
			rig.ApplyImpulse( new Vector3( 0, 800f * (ENGINE.progress + 5f) / 100f, 100f ) * x.Transform.Rotation );
			rig.ApplyTorque( new Vector3( 4000f ) );
			x.Transform.ClearInterpolation();
		} );
		BreakSpin();
	}

	public void BreakSpin()
	{
		IsAttached = false;
		BigBodyBox.Scale = new Vector3( 0 );
		TIMER.Update();
	}
	delegate void ResetAll();
	ResetAll RestartAllBlades;
	public void RestartSpin()
	{
		IsAttached = true;
		if ( RestartAllBlades == null ) Log.Error("blades init error");
			RestartAllBlades();
		BigBodyBox.Scale = new Vector3( 250 );
		TIMER.Update();
	}

	public void ApplySaveState( SaveState state )
	{
		if ( state.SpinAttached )
		{
			if ( !IsAttached ) RestartSpin();
			return;
		}
		BreakSpin();
		blades.ForEach( x =>
		{
			x.GameObject.Parent = PLAYEROBJ;
			x.Transform.Position = new Vector3( 9999999, 9999999, 9999999 );
			x.Transform.ClearInterpolation();
		} );
	}
}
