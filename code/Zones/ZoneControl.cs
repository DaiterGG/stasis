using System;
using Stasis.Data;
using Stasis.UI;
using static Sandbox.Curve;

namespace Stasis.Zones;
public sealed class ZoneControl : Component
{
	private Color CurrentColor;
	[Property] public GameObject LinePrefab;

	public GameTransform StartPoint;
	private GameTransform _spawnPoint;
	public GameTransform SpawnPoint
	{
		get => _spawnPoint == null ? StartPoint : _spawnPoint;
		set => _spawnPoint = value;
	}
	public List<IZone> Zones;
	public List<Beacon> Beacons;
	public Dictionary<int, List<Beacon>> GroupedBeacons;
	public int BeaconsActivated;
	Sng SNG;
	FileControl FC;
	Timer TIMER;
	MenuController MENUC;
	SaveStateControl STATE;
	public void OnAwakeInit()
	{
		SNG = Sng.Inst;
		FC = SNG.FileC;
		TIMER = SNG.Timer;
		MENUC = SNG.MenuC;
		STATE = SNG.StateC;
		ZonesClearAll();
	}

	/// <summary>
	/// Called On Map Change
	/// </summary>
	public void ZonesClearAll()
	{
		StartPoint = null;
		SpawnPoint = null;
		Zones = new List<IZone>();
		Beacons = new List<Beacon>();
		GroupedBeacons = new Dictionary<int, List<Beacon>>();
		BeaconsActivated = 0;
	}
	delegate void ResetAllZones( bool activate );
	ResetAllZones ResetDelegate;
	/// <summary>
	/// Called if any zones found
	/// </summary>
	public void MapInit()
	{
		try
		{
			foreach ( var zone in Zones )
			{
				if ( zone is Component comp )
				{
					TryDecorateBox( comp.GameObject );
					if ( zone is Beacon )
					{
						Beacons.Add( (Beacon)zone );
					}
				}
			}
			BeaconsInit();
			ZonesReset();
		}
		catch ( Exception e )
		{
			Log.Warning( "Failed to decorate zones: " + e.Message );
		}
	}
	/// <summary>
	/// Called on Restart and After Map Init to set colors
	/// </summary>
	public void ZonesReset()
	{
		BeaconsActivated = 0;
		if ( ResetDelegate == null ) return;
		ResetDelegate( false );
	}
	void BeaconsInit()
	{
		ResetDelegate = null;
		foreach ( var b in Beacons )
		{
			ResetDelegate += b.Activate;
			if ( GroupedBeacons.ContainsKey( b.ID ) )
			{
				GroupedBeacons[b.ID].Add( b );
			}
			else
			{
				GroupedBeacons.Add( b.ID, new List<Beacon> { b } );
			}
		}
	}
	public void BeaconAcitvate( int id )
	{
		if ( !GroupedBeacons.ContainsKey( id ) ) return;
		var group = GroupedBeacons[id];
		if ( group.Count == 0 ) return;
		if ( !group.First().IsActivated ) BeaconsActivated++;
		foreach ( var b in group )
		{
			if ( !b.IsActivated ) b.Activate();
		}
	}
	public int[] GetActiveBeacons(){
		var list = new int[BeaconsActivated];
		int i = 0;
		foreach ( var b in GroupedBeacons ){
			if (b.Value.Count == 0) continue;
			if (b.Value.First().IsActivated){
				list[i] = b.Key;
				i++;
			}
		}
		return list;
	}
	bool BeaconsComplete()
	{
		if ( BeaconsActivated == GroupedBeacons.Count ) return true;
		return false;
	}
	public void EndZoneEnter( GameObject go, Collider cof )
	{
		if ( !BeaconsComplete() ) return;
		if ( TIMER.IsRunning )
		{

			TIMER.TimerFinish();
			FC.SetScore();
			MENUC.ShowEndScreen(TIMER.timerSeconds);
		} else if (STATE.Enabled && STATE.IsRunning){
			MENUC.ShowEndScreen(STATE.CurrentTime);
		}
	}
	void TryDecorateBox( GameObject box )
	{
		var col = box.Components.Get<BoxCollider>();
		if ( col == null ) return;
		var decor = box.Components.Get<AutoDecor>();
		if ( decor == null ) return;
		if ( !decor.AutoDecorate ) return;
		CurrentColor = decor.ColorOfTheLines;
		CreateBoxEdges( col, decor.LineWidth );
	}
	/*public void LegacyZoneCreate()
            EndZones.Add( EndZonePrefab.Clone( GameObject, zone.Transform.Position, zone.Transform.Rotation, zone.Transform.Scale ) );
            EndZones.Last().Enabled = true;
            var box = zone.Components.Get<EnvmapProbe>();
            var col = EndZones.Last().Components.Get<BoxCollider>();
            col.Center = box.Bounds.Center;
            col.Scale = box.Bounds.Size;
            zone.Enabled = false;
			CurrentColor = endZoneColor;
			CreateBoxEdges( EndZones.Last(), box );
    */
	void CreateBoxEdges( BoxCollider box, float width )
	{
		Vector3[] c = new Vector3[8];

		Vector3[] offsets = new Vector3[]
		{
			new Vector3(-1, -1, -1),
			new Vector3( 1, -1, -1),
			new Vector3( 1,  1, -1),
			new Vector3(-1,  1, -1),
			new Vector3(-1, -1,  1),
			new Vector3( 1, -1,  1),
			new Vector3( 1,  1,  1),
			new Vector3(-1,  1,  1)
		};

		for ( int i = 0; i < 8; i++ )
		{
			c[i] = box.Transform.Position + box.Center + offsets[i] * (box.Scale * 0.5f) * (box.Transform.Scale);
		}

		int[,] edges = new int[,]
		{
			{0, 1}, {1, 2}, {2, 3}, {3, 0},
			{4, 5}, {5, 6}, {6, 7}, {7, 4},
			{0, 4}, {1, 5}, {2, 6}, {3, 7}
		};

		for ( int i = 0; i < edges.GetLength( 0 ); i++ )
		{
			CreateLine( c[edges[i, 0]], c[edges[i, 1]], width );
		}
	}
	void CreateLine( Vector3 p1, Vector3 p2, float width )
	{
		var l = LinePrefab.Clone();

		l.Parent = Scene;
		l.Enabled = true;
		var rend = l.Components.Get<LineRenderer>();
		rend.Color = new Gradient( new Gradient.ColorFrame( 0, CurrentColor ) );
		rend.VectorPoints[0] = p1;
		rend.VectorPoints[1] = p2;

		rend.Width = rend.Width.WithFrames( new List<Frame>() { new Frame( 0, width ) } );
	}
}
