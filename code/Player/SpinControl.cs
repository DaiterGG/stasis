using System;
using Stasis.Data;

namespace Stasis.Player;

public sealed class SpinControl : Component
{
	[Property] public Rigidbody PropRig;
	[Property, Range( 0, 10000f, 100f )] public float BladeGravity {get; set;} = 1500f;
	EngineComponent ENGINE;
	GameObject PLAYEROBJ;
	Timer TIMER;
	float speedMult = 0.2f;
	public bool isAttached;
	List<SpinTrigger> blades = new List<SpinTrigger>();
	public void OnAwakeInit()
	{

		ENGINE = Sng.Inst.Player.Engine;
		TIMER = Sng.Inst.Timer;
		PLAYEROBJ = Sng.Inst.Player.GameObject;
		PropRig.GameObject.Children.ToList().ForEach( x =>
		{
			var t = x.Components.Get<SpinTrigger>();
			if ( x.Enabled && t != null ) blades.Add( t );
		} );
		RestartSpin();
	}
	public void OnFixedGlobal()
	{
		foreach ( var blade in blades )
		{
			blade.OnFixedGlobal();
		}
		if ( !isAttached ) return;
		if ( ENGINE.isRunning )
		{
			PropRig.Transform.Rotation *= Rotation.From( 0, ENGINE.gain / ENGINE.gravity * 100 * speedMult, 0 );
		}
		else PropRig.Transform.Rotation *= Rotation.From( 0, ENGINE.progress * speedMult, 0 );

	}
	public void SpinCollision()
	{
		if ( !isAttached ) return;

		blades.ForEach( x =>
		{
			x.GameObject.Parent = PLAYEROBJ;
			x.Transform.Position = PropRig.Transform.Position;
			var rig = x.Components.Get<Rigidbody>();
			rig.ApplyImpulse( new Vector3( 0, 800f * (ENGINE.progress + 5f) / 100f, 100f ) * x.Transform.Rotation );
			rig.ApplyTorque( new Vector3( 4000f ) );
			x.Transform.ClearInterpolation();
		} );
		isAttached = false;
	}
	public void RestartSpin()
	{
		isAttached = true;
	}

	public void ApplySaveState( SaveState state )
	{
		if (state.SpinAttached)
			isAttached = true;
		else 
		{
			isAttached = false;
			blades.ForEach( x =>
			{
				x.GameObject.Parent = PLAYEROBJ;
				x.Transform.Position = new Vector3(9999999,9999999,9999999);
				x.Transform.ClearInterpolation();
			} );
		}
	}
}
