using System;

namespace Sandbox;
public sealed class ZoneCreate : Component
{
	private Color endZoneColor = new Color( 1f, 0, 0 );
	private Color CurrentColor;
	[Property] public GameObject EndZonePrefab;
	[Property] public GameObject LinePrefab;
	public List<GameObject> EndZones = new List<GameObject>();
	Sng SNG;
	public void OnAwakeInit()
	{
		SNG = Sng.Inst;
	}
	public void MapInit()
	{
		try
		{
			foreach ( var zone in SNG.EndZones )
			{
				DecorateBox( zone );
			}
		}
		catch ( Exception e )
		{
			Log.Warning( "Failed to decorate zones: " + e.Message );
		}
	}
	public void DecorateBox( GameObject box )
	{
		var col = box.Components.Get<BoxCollider>();
		if ( col == null ) return;
		var decor = box.Components.Get<AutoDecor>();
		if ( decor == null ) return;
		if ( !decor.AutoDecorate ) return;
		CurrentColor = decor.ColorOfTheLines;
		CreateBoxEdges( col );
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
	private void CreateBoxEdges( BoxCollider box )
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
			c[i] = box.Transform.Position + box.Center + offsets[i] * (box.Scale * 0.5f);
		}

		int[,] edges = new int[,]
		{
			{0, 1}, {1, 2}, {2, 3}, {3, 0},
			{4, 5}, {5, 6}, {6, 7}, {7, 4},
			{0, 4}, {1, 5}, {2, 6}, {3, 7}
		};

		for ( int i = 0; i < edges.GetLength( 0 ); i++ )
		{
			CreateLine( c[edges[i, 0]], c[edges[i, 1]] );
		}
	}
	private void CreateLine( Vector3 p1, Vector3 p2 )
	{
		var l = LinePrefab.Clone();

		l.Parent = GameObject.Parent;
		l.Enabled = true;
		var rend = l.Components.Get<LineRenderer>();
		//Log.Info( rend.Width.AddOrReplacePoint( new Curve.Frame( 0f, 7f, 1f, 1f ) ) );
		rend.Color = new Gradient( new Gradient.ColorFrame( 0, CurrentColor ) );
		rend.VectorPoints[0] = p1;
		rend.VectorPoints[1] = p2;
	}
}
