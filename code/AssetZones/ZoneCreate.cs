using System;
namespace Sandbox;
public sealed class ZoneCreate : Component
{
	private Color endZoneColor = new Color( 1f, 0, 0);
	private Color CurrentColor;
    [Property] public GameObject EndZonePrefab;
    [Property] public GameObject LinePrefab;
    public List<GameObject> EndZones = new List<GameObject>();
	Sng SNG;
	protected override void OnAwake()
	{
		SNG = Sng.Inst;
	}
	protected override void OnStart()
	{
		base.OnStart();
        foreach ( var zone in SNG.EndZones )
        {
            EndZones.Add( EndZonePrefab.Clone( GameObject, zone.Transform.Position, zone.Transform.Rotation, zone.Transform.Scale ) );
            EndZones.Last().Enabled = true;
            var box = zone.Components.Get<EnvmapProbe>();
            var col = EndZones.Last().Components.Get<BoxCollider>();
            col.Center = box.Bounds.Center;
            col.Scale = box.Bounds.Size;
            zone.Enabled = false;
			CurrentColor = endZoneColor;
			CreateBoxEdges( EndZones.Last(), box );
		}
	}

    private void CreateBoxEdges( GameObject parent, EnvmapProbe box )
    {
		Vector3[] c = box.Bounds.Corners.ToArray();
		for ( int x = 0; x < c.Length; x++ ) c[x] += box.Transform.Position;
		
		CreateLine( parent, c[0], c[1] );
		CreateLine( parent, c[1], c[2] );
		CreateLine( parent, c[2], c[3] );
		CreateLine( parent, c[3], c[0] );
		CreateLine( parent, c[4], c[5] );
		CreateLine( parent, c[5], c[6] );
		CreateLine( parent, c[6], c[7] );
		CreateLine( parent, c[7], c[4] );
		CreateLine( parent, c[0], c[4] );
		CreateLine( parent, c[1], c[5] );
		CreateLine( parent, c[2], c[6] );
		CreateLine( parent, c[3], c[7] ); //kill me
	}
	private void CreateLine(GameObject parent, Vector3 p1, Vector3 p2)
	{
		var l = LinePrefab.Clone();
		l.Parent = parent;
		l.Enabled = true;
		var rend = l.Components.Get<LineRenderer>();
		rend.Color = new Gradient( new Gradient.ColorFrame( 0, CurrentColor) );
		rend.VectorPoints[0] = p1;
		rend.VectorPoints[1] = p2;
	}
}
