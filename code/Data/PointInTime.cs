using System.Numerics;

namespace Stasis.Data;

public struct PointInTime
{
	public readonly Vector3 Transform;
	public readonly short Yaw;
	public readonly short Pitch;
	public readonly short Roll;
	public PointInTime(Vector3 tr, short y, short p, short r){
		Transform = tr;
		Yaw = y;
		Pitch = p;
		Roll = r;
	}
}
 public static class PointInTimeControl
 {
	static void ApplySaveState( PointInTime point, Transform player ){
		player.Position = point.Transform;
		player.Rotation = Rotation.From(
			point.Yaw / 100f,
			point.Pitch / 100f,
			point.Roll / 100f
		);
	}
	static PointInTime GetSaveState( Transform player ){
		return new PointInTime(
			player.Position,
			(short)(player.Rotation.Yaw() * 100),
			(short)(player.Rotation.Pitch() * 100),
			(short)(player.Rotation.Roll() * 100)
		);
	}
 }
