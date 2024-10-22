using System;
using Stasis.Data;
using Stasis.Player;
using Stasis.UI;
using static Sandbox.Curve;

namespace Stasis.Zones;
public sealed class ZoneControl : Component
{
    [Property] public GameObject LinePrefab;

    public Transform? StartPoint;
    private Transform? _spawnPoint;
    public Transform? SpawnPoint
    {
        get => _spawnPoint == null ? StartPoint : _spawnPoint;
        set => _spawnPoint = value;
    }
    public List<IZone> Zones { get; set; }
    public Dictionary<int, List<Beacon>> GroupedBeacons { get; private set; }
    public int BeaconsActivatedCount { get; private set; }
    Dictionary<int, CheckPoint> CheckPoints;
    public int? CheckPointActivated { get; private set; }
    Sng SNG;
    FileControl FC;
    Timer TIMER;
    MenuController MENUC;
    SaveStateControl STATE;
    SpinControl SPIN;
    EngineComponent ENGINE;
    public void OnAwakeInit()
    {
        SNG = Sng.Inst;
        FC = SNG.FileC;
        TIMER = SNG.Timer;
        MENUC = SNG.MenuC;
        STATE = SNG.StateC;
        SPIN = SNG.Player.SpinC;
        ENGINE = SNG.Player.Engine;
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
        GroupedBeacons = new Dictionary<int, List<Beacon>>();
        CheckPoints = new Dictionary<int, CheckPoint>();
        CheckPointActivated = null;
        BeaconsActivatedCount = 0;
        ResetDelegate = null;
    }
    delegate void ResetAllZones(bool activate);
    ResetAllZones ResetDelegate;
    /// <summary>
    /// Called if any zones found
    /// </summary>
    public void ZoneInit()
    {
        try
        {
            foreach (var zone in Zones)
            {
                if (zone is Component comp)
                {
                    TryDecorateBox(comp.GameObject);
                    if (zone is Beacon beacon)
                    {
                        ResetDelegate += beacon.ActivateToggle;
                        InsertAtId(GroupedBeacons, beacon);
                    }
                    if (zone is CheckPoint check)
                    {
                        check.ID = CheckPoints.Count;
                        InsertAtId(CheckPoints, check);
                    }
                }
            }
            ZonesReset();
        }
        catch (Exception e)
        {
            Log.Warning("Failed to zone init " + e.Message);
        }
    }

    /// <summary>
    /// Called on Restart and After Map Init to set colors
    /// </summary>
    public void ZonesReset(int? checkpointActivated = null)
    {
        BeaconsActivatedCount = 0;
        CheckPointActivated = checkpointActivated;
        if (ResetDelegate == null) return;
        ResetDelegate(false);
    }
    public void BeaconAcitvate(int id)
    {
        if (!GroupedBeacons.ContainsKey(id)) return;
        var group = GroupedBeacons[id];
        if (group.Count == 0) return;
        if (!group.First().IsActivated) BeaconsActivatedCount++;
        foreach (var beacon in group)
        {
            if (!beacon.IsActivated) beacon.ActivateToggle();
        }
    }
    public int[] GetActiveBeacons()
    {
        var list = new int[BeaconsActivatedCount];
        int i = 0;
        foreach (var group in GroupedBeacons)
        {
            if (group.Value.Count == 0) continue;
            if (group.Value.First().IsActivated)
            {
                list[i] = group.Key;
                i++;
            }
        }
        return list;
    }
    bool BeaconsComplete()
    {
        if (BeaconsActivatedCount == GroupedBeacons.Count) return true;
        return false;
    }
    public void CheckPointActivate(int id)
    {
        if (!CheckPoints.ContainsKey(id)) return;
        CheckPointActivated = id;
    }
    public void EndZoneEnter(GameObject go, Collider cof)
    {
        if (!BeaconsComplete())
        {
            ZoneBlockedMessage();
            return;
        }
        if (TIMER.IsRunning)
        {

            TIMER.TimerFinish();
            FC.SetScore();
            MENUC.ShowEndScreen(TIMER.timerSeconds);
        }
        else if (STATE.Enabled && STATE.IsRunning)
        {
            MENUC.ShowEndScreen(STATE.CurrentTime);
        }
    }
    public bool TrySoftRestart()
    {
        if (CheckPointActivated is not int i) return false;
        TeleportPlayer(CheckPoints[i].Spawn.WorldTransform, true, true, true);
        return true;
    }
    public void ApplySaveState(SaveState state)
    {
        ZonesReset(state.CheckPointActivated);
        foreach (var beacon in state.ActiveBeacons)
        {
            BeaconAcitvate(beacon);
        }
    }
    public void TeleportPlayer(Transform trans, bool resetVel, bool resetRotation, bool repairPropeller)
    {
        var state = SaveStateControl.GetSaveState(TIMER.timerSeconds);
        state.Transform = trans.Position;
        if (resetVel)
        {
            state.Velocity = Vector3.Zero;
            state.AngularVelocity = Vector3.Zero;
        }
        if (resetRotation) state.Rotation = trans.Rotation;
        if (repairPropeller) state.SpinAttached = true;
        SaveStateControl.ApplySaveState(state);
    }
    public void ZoneBlockedMessage()
    {
        MENUC.IngameUI.BeaconPopup();
    }
    void TryDecorateBox(GameObject box)
    {
        var col = box.Components.Get<BoxCollider>();
        if (col == null) return;
        var decor = box.Components.Get<AutoDecor>();
        if (decor == null) return;

        CreateBoxEdges(col, decor);
    }
    /*public void LegacyZoneCreate()
            EndZones.Add( EndZonePrefab.Clone( GameObject, zone.WorldPosition, zone.WorldRotation, zone.Transform.Scale ) );
            EndZones.Last().Enabled = true;
            var box = zone.Components.Get<EnvmapProbe>();
            var col = EndZones.Last().Components.Get<BoxCollider>();
            col.Center = box.Bounds.Center;
            col.Scale = box.Bounds.Size;
            zone.Enabled = false;
            CurrentColor = endZoneColor;
            CreateBoxEdges( EndZones.Last(), box );
    */
    void CreateBoxEdges(BoxCollider box, AutoDecor decor)
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

        for (int i = 0; i < 8; i++)
        {
            c[i] = box.WorldPosition + box.Center + offsets[i] * (box.Scale * 0.5f) * box.WorldScale;
        }

        int[,] edges = new int[,]
        {
            {0, 1}, {1, 2}, {2, 3}, {3, 0},
            {4, 5}, {5, 6}, {6, 7}, {7, 4},
            {0, 4}, {1, 5}, {2, 6}, {3, 7}
        };

        for (int i = 0; i < edges.GetLength(0); i++)
        {
            CreateLine(c[edges[i, 0]], c[edges[i, 1]], decor);
        }
    }
    void CreateLine(Vector3 p1, Vector3 p2, AutoDecor decor)
    {
        var l = LinePrefab.Clone();

        l.Parent = Scene;
        l.Enabled = true;
        var line = l.Components.Get<LineRenderer>();
        line.Color = new Gradient(new Gradient.ColorFrame(0, decor.ColorOfTheLines));
        line.VectorPoints[0] = p1;
        line.VectorPoints[1] = p2;
        var width = decor.LineWidth;
        if (decor.WireFrame)
        {
            width = 0;
            line.Wireframe = true;
        }
        else line.Wireframe = false;
        line.Width = line.Width.WithFrames(new List<Frame>() { new Frame(0, width) });
    }
    void InsertAtId<Item, Zone>(Dictionary<int, Item> diction, Zone zone)
    {
        if (zone is not IZone z) return;
        if (diction is Dictionary<int, Zone> dictionOfZones)
        {
            if (dictionOfZones.ContainsKey(z.ID))
                dictionOfZones[z.ID] = zone;
            else dictionOfZones.Add(z.ID, zone);
        }
        else if (diction is Dictionary<int, List<Zone>> dictionoflists)
        {
            if (dictionoflists.ContainsKey(z.ID))
                dictionoflists[z.ID].Add(zone);
            else dictionoflists.Add(z.ID, new List<Zone> { zone });
        }
    }
}
