using System;
using Stasis.Data;
using Stasis.Player;
using Stasis.UI;
using Stasis.Zones;
namespace Stasis;
public sealed class Sng : Component
{
    private static Sng _sng;
    public static Sng Inst { get { return _sng; } }

    public GameState GameState { get; set; }

    [Property] public MenuController MenuC;
    [Property] public PlayerComp Player;
    [Property] public ZoneControl ZoneC;
    public RecordReplay RecordC = new();
    public ViewReplay ViewC = new();
    public LBControl LB = new();
    public FileControl FileC = new();
    public Timer Timer = new();
    public SaveStateControl StateC = new();
    public bool firstTime { get; set; } = true;
    public bool blurOn { get; set; }
    protected override void OnAwake()
    {
        // Game.IsPaused = true;
        // Log.Info(_sng.ToString());
        if (_sng == null || "Sng on (null)" == _sng.ToString())
        {
            _sng = this;
        }
        else
        {
            ELog("Second game instance is found, deleting");
            GameObject.Parent.Enabled = false;
            GameObject.Parent.DestroyImmediate();
            return;
        }


        blurOn = !Game.IsEditor;
        OnAwakeInit();
    }
    public void OnAwakeInit()
    {
        Timer.OnAwakeInit();
        StateC.OnAwakeInit();
        FileC.OnAwakeInit();
        MenuC.OnAwakeInit();
        Player.Engine.OnAwakeInit();
        Player.SoundC.OnAwakeInit();
        Player.SpinC.OnAwakeInit();
        ZoneC.OnAwakeInit();
        Player.CameraC.OnAwakeInit();
        RecordC.OnAwakeInit();
        ViewC.OnAwakeInit();
    }

    protected override void OnStart()
    {
        FileC.OnStartInit();
        //File.FetchNewMap( "move.stasis_playgr", "official" );
        //File.FetchNewMap( "dicta.base", "community" );
        //File.DownloadAndLoad( "move.stasis_playground", true );

        if (Game.IsEditor && Scene.Name != "Scene")
        {
            ELog("Testing local map");
            FileC.SetCurrentMap(FileC.OfficialMaps.First());
            MapInit();
        }
        else
        {
            FileC.DownloadAndLoad(FileC.OfficialMaps.First());
        }
        base.OnStart();
    }
    /// <summary>
    /// Called on every map load
    /// </summary>
    private void MapInit()
    {
        Info MapInfo = null;

        ZoneC.ZonesClearAll();
        var heap = Scene.GetAllObjects(true);
        foreach (var obj in heap)
        {
            if (obj.Name.ToLower().Contains("spawn") && obj.Name.ToLower().Contains("player"))
            {
                ZoneC.SpawnPoint = obj.WorldTransform;
            }
            else if (obj.Name.ToLower().Contains("start") && obj.Name.ToLower().Contains("player"))
            {
                ZoneC.StartPoint = obj.WorldTransform;
            }
            else if (obj.Name == "Map Info")
            {
                MapInfo = obj.Components.Get<Info>();
                if (MapInfo == null)
                    ELog("no info component found");
                else
                    ELog("Info Found");
            }
            else
            {
                var zone = obj.Components.Get<IZone>();
                if (zone != null)
                    ZoneC.Zones.Add(zone);
            }
        }
        if (MapInfo == null) Log.Warning("Info Not Found");

        FileC.InfoSerialize(MapInfo);

        if (ZoneC.Zones.Count == 0) Log.Warning("No Zones Found");
        else ZoneC.ZoneInit();
        if (ZoneC.StartPoint == null) Log.Warning("Start Not Found");
        ChangeGameState(GameState.MainMenu);
    }
    protected override void OnFixedUpdate()
    {
        OnFixedUpdateSequence();
    }
    private void OnFixedUpdateSequence()
    {
        try
        {
            MenuC.BlackUI.Opacity -= 0.005f;
            if (GameState == GameState.Play)
            {
                if (Input.Pressed("SoftRestart"))
                {
                    if (!ZoneC.TrySoftRestart())
                        PlayerHardRestart();
                }
                if (Input.Pressed("Restart"))
                {
                    PlayerHardRestart();
                }
                if (Input.Pressed("SelfDestruct"))
                {
                    if (Player.CameraC.FreeCam.GameObject.Enabled)
                    {
                        Player.CameraC.TeleportToFreecam();
                    }
                    else
                    {
                        Player.JumpC.TryToJump();
                        Player.SpinC.SpinCollision();
                    }
                }
                if (Input.Pressed("Back") || Input.EscapePressed)
                {
                    ChangeGameState(GameState.MainMenu);
                }
                if (Input.Pressed("HideUI"))
                {
                    MenuC.InGameUIToggle();
                }
                if (Input.Pressed("ShowInfo"))
                {
                    MenuC.InGameHelpToggle();
                }
            }
            else if (GameState == GameState.MainMenu)
            {
                if (Input.Pressed("SelfDestruct"))
                {
                    MenuC.PlayPressed();
                }
                if (Input.Pressed("Quit"))
                {
                    MenuC.Quit();
                }
            }
            else if (GameState == GameState.ViewReplay)
            {
                if (Input.Pressed("HideUI"))
                {
                    MenuC.ReplayUIToggle();
                }
                if (Input.Pressed("Back"))
                {
                    MenuC.ReplayUI.GoBack();
                }
            }

            StateC.OnFixedGlobal();
            Player.SoundC.OnFixedGlobal();
            Player.SpinC.OnFixedGlobal();
            Player.Engine.OnFixedGlobal();
            if (GameState == GameState.Play || GameState == GameState.ViewReplay) Timer.OnFixedGlobal();
            Player.CameraC.OnFixedGlobal();
            RecordC.OnFixedGlobal();
        }
        catch (Exception e) { LogOnce("Main fixed error" + e.StackTrace); }
    }
    protected override void OnUpdate()
    {
        Timer.OnUpdateGlobal();
        StateC.OnUpdateGlobal();
        ViewC.OnUpdateGlobal();
    }
    public void LoadNewMap(SceneFile file)
    {

        Log.Info("Map trying to load: " + file.ResourceName);
        try
        {
            Scene.Load(file);
        }
        catch (Exception e) { Log.Warning("Map not found localy: " + e.Message); }

        MapInit();
    }
    public void PlayerHardRestart()
    {
        TeleportPlayer(ZoneC.StartPoint);
        StateC.Reset();
        ZoneC.ZonesReset();
        Player.SpinC.RestartSpin();
        Timer.Reset();
    }
    public void ChangeGameState(GameState state)
    {
        GameState = state;

        Player.CameraC.FreeCamEnable(false);
        Player.SpinC.RestartSpin();
        ViewC.PauseView();
        RecordC.Pause();
        ZoneC.ZonesReset();

        MenuC.ReplayUI.GameObject.Enabled = false;

        MenuC.GameUIVisible = false;
        StateC.Enabled = false;
        switch (state)
        {
            case GameState.Play:
                Timer.Reset();
                MenuC.GameUIVisible = true;
                MenuC.CloseMenu();
                TeleportPlayer(ZoneC.StartPoint);
                break;
            case GameState.MainMenu:
                if (ZoneC.SpawnPoint == null)
                {
                    Log.Warning("No spawn and no start point");
                    ZoneC.SpawnPoint = Player.WorldTransform;
                    ZoneC.StartPoint = Player.WorldTransform;
                }
                TeleportPlayer(ZoneC.SpawnPoint);
                Player.Transform.ClearInterpolation();
                Player.Engine.ResetPos(true);
                MenuC.OpenMenu();
                MenuC.SetCameraLook();
                break;
            case GameState.ViewReplay:
                MenuC.CloseMenu();
                MenuC.ReplayUI.GameObject.Enabled = true;
                break;
        }
    }
    public void TeleportPlayer(Transform? pos)
    {
        if (pos is Transform truePos)
        {
            Player.WorldPosition = truePos.Position;
            Player.WorldRotation = truePos.Rotation;
        }
        else
        {
            if (ZoneC.SpawnPoint is not Transform foo)
            {
                Log.Warning("No spawn and no start point");
                ZoneC.SpawnPoint = Player.WorldTransform;
                ZoneC.StartPoint = Player.WorldTransform;
            }
            if (ZoneC.SpawnPoint is Transform t)
            {
                Player.WorldPosition = t.Position;
                Player.WorldRotation = t.Rotation;
            }
        }
        Player.Transform.ClearInterpolation();
        Player.Engine.ResetPos(false);
    }

    public string FormatTime(float totalSeconds)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(totalSeconds);

        string formattedTime = "";

        if (timeSpan.Hours > 0)
        {
            formattedTime += $"{timeSpan.Hours:D2}:";
        }
        else if (timeSpan.Minutes > 0 || timeSpan.Hours > 0)
        {
            formattedTime += $"{timeSpan.Minutes:D2}:";
        }
        formattedTime += $"{timeSpan.Seconds:D2}.{timeSpan.Milliseconds:D3}";

        if (formattedTime[0] == '0' && formattedTime[1] != '.')
            formattedTime = formattedTime.Substring(1);

        return formattedTime;
    }
    //unused
    private void FindMaps()
    {
        var maps = FileSystem.Mounted.FindFile("/", "*.vpk");
        foreach (var map in maps)
        {
            //  Log.Info( map );
        }

    }
    /// <summary>
    /// Log. for Editor Mode only
    /// type = "Log", "Warn", "Err"
    /// </summary>
    public static void ELog(object s, string type = "Log")
    {
        if (Game.IsEditor)
        {
            if (type == "Log")
                Log.Info(s);
            else if (type == "Warn")
                Log.Warning(s);
            else if (type == "Err")
                Log.Error(s);
        }
    }
    string lastLog;
    public static void LogOnce(object s, bool editLog = true)
    {
        if (Inst.lastLog != s.ToString())
        {
            if (editLog)
            {
                ELog(s);
            }
            else Log.Info(s);
            Inst.lastLog = s.ToString();
        }
    }
}
public enum GameState
{
    Play,
    MainMenu,
    ViewReplay
}

