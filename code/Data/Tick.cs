using System.Numerics;

namespace Stasis.Data;

public struct Tick
{
	public readonly Vector3 Transform;
	public readonly short Yaw;
	public readonly short Pitch;
	public readonly short Roll;
	public Tick(Vector3 tr, short y, short p, short r){
		Transform = tr;
		Yaw = y;
		Pitch = p;
		Roll = r;
	}
}
 /*public static class TickControl
 {
	static void ApplySaveState( Tick point, Transform player ){
		player.Position = point.Transform;
		player.Rotation = Rotation.From(
			point.Yaw / 100f,
			point.Pitch / 100f,
			point.Roll / 100f
		);
	}
	static Tick GetSaveState( Transform player ){
		return new Tick(
			player.Position,
			(short)(player.Rotation.Yaw() * 100),
			(short)(player.Rotation.Pitch() * 100),
			(short)(player.Rotation.Roll() * 100)
		);
	}
 }*/
