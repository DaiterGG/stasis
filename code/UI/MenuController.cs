using System;
using Sandbox.Audio;
using Stasis.Data;
using Stasis.Player;
using Sandbox.UI;
namespace Stasis.UI;

public sealed class MenuController : Component
{
    [Property] public MainMenu MenuUI { get; set; }
    [Property] public IngameUI IngameUI { get; set; }
    [Property] public EndScreen EndUI { get; set; }
    [Property] public SettingsUI SetUI { get; set; }
    [Property] public ChooseMenu ChooseUI { get; set; }
    [Property] public ScreenPanel BlackUI { get; set; }
    [Property] public GameObject Camera { get; set; }
    [Property] public ReplayUI ReplayUI { get; set; }
    public float pitchOffset = -11.3f;
    public float yawOffset = 13.111f;
    public Vector3 CameraPos;


    public float speed = 0;
    public bool MainMenuIsOpen = false;
    GameObject BODY;
    SpinControl SPINC;
    Timer TIMER;
    EngineComponent ENGINE;
    Sng SNG;
    FileControl FC;

    public void OnAwakeInit()
    {
        SNG = Sng.Inst;
        SPINC = SNG.Player.SpinC;
        TIMER = SNG.Timer;
        BODY = SNG.Player.Body;
        ENGINE = SNG.Player.Engine;
        FC = SNG.FileC;
        //Set camera angle to use for all other menues
        SetCameraOffset();
        Camera.Enabled = true;
        ApplySettings();
        ShowInfo();
    }
    protected override void OnUpdate()
    {
        if (!MainMenuIsOpen)
        {
            return;
        }
        if (Input.Down("attack1") || Input.Down("attack2"))
        {
            speed += Mouse.Delta.x / 2000f;
        }

        var rad = Camera.WorldPosition - BODY.WorldPosition;
        var sin = (float)Math.Sin(speed);
        var cos = (float)Math.Cos(speed);
        var x = rad.x * cos - rad.y * sin;
        var y = rad.x * sin + rad.y * cos;

        Camera.WorldPosition = new Vector3(x, y, Camera.WorldPosition.z) + BODY.WorldPosition * new Vector3(1, 1, 0);
        //Camera.WorldRotation = Rotation.FromYaw(Mouse.Delta.x / 100) + Camera.WorldRotation;
        SetCameraLook();
        speed *= 0.93f;
    }
    public void SetCameraOffset()
    {
        CameraPos = Camera.LocalPosition;
    }
    public void CameraInit()
    {
        Camera.Enabled = true;
        Camera.LocalPosition = CameraPos;

        SetCameraLook();
        Camera.Transform.ClearInterpolation();
    }
    public void SetCameraLook()
    {
        var rad = Camera.WorldPosition - BODY.WorldPosition;
        Camera.WorldRotation = Rotation.LookAt(rad * new Vector3(-1, -1, 0)) * Rotation.FromYaw(yawOffset) * Rotation.FromPitch(pitchOffset);

    }
    public void PlayPressed()
    {
        SNG.ChangeGameState(GameState.Play);
    }
    public void CloseMenu()
    {
        Camera.Enabled = false;
        IngameUI.GameObject.Enabled = true;
        MenuUI.GameObject.Enabled = false;
        MainMenuIsOpen = false;
    }
    public void OpenMenu()
    {
        Camera.Enabled = true;
        IngameUI.GameObject.Enabled = false;
        GameUIVisible = false;
        MainMenuIsOpen = true;
        ChooseUI.GameObject.Enabled = false;
        MenuUI.GameObject.Enabled = true;
        HelpVisible = false;
        CameraInit();
    }
    public void Controls()
    {
        Game.Overlay.ShowBinds();
    }
    public void ChooseMenuPressed()
    {
        UpdateMapsList();
        ChooseUI.GameObject.Enabled = true;
        MenuUI.GameObject.Enabled = false;
    }
    public void CloseChooseMenu()
    {
        ChooseUI.GameObject.Enabled = false;
        MenuUI.GameObject.Enabled = true;
    }
    public void SettingsPressed()
    {
        SetUI.GameObject.Enabled = true;
        MenuUI.GameObject.Enabled = false;

    }
    public void CloseSettings()
    {
        SetUI.GameObject.Enabled = false;
        MenuUI.GameObject.Enabled = true;
        FileControl.SaveSettings();
    }
    public void ViewReplayPressed()
    {
        SNG.ChangeGameState(GameState.ViewReplay);
    }

    public void MapSelect()
    {
        try
        {
            Game.Overlay.ShowPackageSelector(/* "type:asset ext:scene " */"type:map stasis", delegate (Package p)
            {
                foreach (var m in FC.OfficialMaps)
                {
                    if (m == p.FullIdent)
                    {
                        FC.FetchNewMap(m, "official");
                        return;
                    }
                }
                foreach (var m in FC.FeaturedMaps)
                {
                    if (m == p.FullIdent)
                    {
                        FC.FetchNewMap(m, "featured");
                        return;
                    }
                }
                FC.FetchNewMap(p.FullIdent, "community");
            });
        }
        catch (Exception e) { Log.Warning(e); }
    }
    public void UpdateMapsList()
    {
        ChooseUI.Official.Clear();
        ChooseUI.Featured.Clear();
        ChooseUI.Community.Clear();
        foreach (var m in FC.Maps)
        {
            if (m.Type == "official")
            {
                ChooseUI.Official.Add(m);
            }
            else if (m.Type == "featured")
            {
                ChooseUI.Featured.Add(m);
            }
            else
            {
                ChooseUI.Community.Add(m);
            }
            //ChooseUI.Official.Add( m );
            //ChooseUI.Community.Add( m );
        }
    }
    public void Quit()
    {
        Game.Close();
    }
    public void ShowEndScreen(float Time)
    {
        // Log.Info("showing end screen");
        if (FC.currentMap == null)
        {
            Log.Warning("no map info loaded?, why there is end_zone?");
            return;
        }
        if (Time == 0)
        {
            Log.Warning("Time = 0");
            return;
        }
        var time = FC.currentMap.Scores[0].Time;
        if (FC.currentMap.SpeedRun)
        {
            EndUI.Gold = SNG.FormatTime(FC.currentMap.GoldTime);
            EndUI.Silver = SNG.FormatTime(FC.currentMap.SilverTime);
            EndUI.Bronze = SNG.FormatTime(FC.currentMap.BronzeTime);

            EndUI.medal = GetMedal(FC.currentMap);
        }
        else
        {
            EndUI.Gold = "";
            EndUI.Silver = "PASS";
            EndUI.Bronze = "";
            EndUI.medal = -1;
        }
        var strtime = SNG.FormatTime(Time);
        EndUI.Time = strtime.Split('.')[0];
        EndUI.TimeMil = "." + strtime.Split('.')[1];
        EndUI.Scores = FC.currentMap.Scores;
        if (FC.currentMap.Scores.Count > 0)
        {
            if (FC.currentMap.Scores[0].Time == Time && FC.currentMap.Scores.Count > 1)
            {
                EndUI.TimeDif = ("-" + SNG.FormatTime(FC.currentMap.Scores[1].Time - Time));
                EndUI.timesave = true;

            }
            else
            {

                EndUI.timesave = false;
                EndUI.TimeDif = ("+" + SNG.FormatTime(Time - FC.currentMap.Scores[0].Time));
            }

        }
        else EndUI.TimeDif = "";

        EndUI.Author = FC.currentMap.Author;
        EndUI.Name = FC.currentMap.Name.ToUpper();

        EndUI.GameObject.Enabled = true;
    }
    public bool GameUIVisible { get; set; } = true;
    public void InGameUIToggle()
    {
        GameUIVisible = !GameUIVisible;
        Sng.ELog(GameUIVisible);
    }
    public bool HelpVisible { get; set; } = false;
    public void InGameHelpToggle()
    {
        HelpVisible = !HelpVisible;
    }
    public int GetMedal(MapData map)
    {
        if (map == null || map.Scores == null || map.Scores.Count == 0 || map.GoldTime == 0) return 0;
        var time = map.Scores[0].Time;
        if (map.SpeedRun)
        {
            if (map.GoldTime > time)
            {
                return 3;
            }
            else if (map.SilverTime > time)
            {
                return 2;
            }
            else if (map.BronzeTime > time)
            {
                return 1;
            }
        }
        else
        {
            return 3;
        }
        return 0;

    }

    public void ApplySettings()
    {
        if (FC.Set.Volume > 10) FC.Set.Volume = 10;
        Mixer.Master.Volume = FC.Set.Volume / 10f;

    }
    public void Copy()
    {
        Clipboard.SetText("https://discord.gg/JNrNHxDE2D");

    }
    public void ShowInfo()
    {
        //if ( infoOnce && SNG.firstTime ) IngameUI.ShowInfo( ControlsInfo() );
        IngameUI.ShowInfo(ControlsInfo());
    }
    private string ControlsInfo()
    {
        var w = Input.GetButtonOrigin("Up").ToUpper();
        var s = Input.GetButtonOrigin("Down").ToUpper();
        var a = Input.GetButtonOrigin("Left").ToUpper();
        var d = Input.GetButtonOrigin("Right").ToUpper();
        var space = Input.GetButtonOrigin("SelfDestruct").ToUpper();
        var b = Input.GetButtonOrigin("Back").ToUpper();
        var f = Input.GetButtonOrigin("SoftRestart").ToUpper();
        var r = Input.GetButtonOrigin("Restart").ToUpper();
        var t = Input.GetButtonOrigin("CameraCycle").ToUpper();
        var c = Input.GetButtonOrigin("FreeCamera").ToUpper();
        var u = Input.GetButtonOrigin("HideUI").ToUpper();
        var h = Input.GetButtonOrigin("ShowInfo").ToUpper();
        var o = Input.GetButtonOrigin("Toggle").ToUpper();
        var m = Input.GetButtonOrigin("Attack2").ToUpper();
        return $"{b} - Back to menu\n" +
            $"{w} {a} {s} {d} - To move\n" +
            $"{f} - Reset to checkpoint\n" +
            $"{r} - Reset to start\n" +
            $"{space} - Self destruct/jump\n" +
            $"{m} - Adjust camera angle\n" +
            $"{t} - Change view\n" +
            $"{c} - Free camera\n" +
            $"{o} - Save state menu\n" +
            $"{u} - Hide UI\n" +
            $"{h} - Hide info\n";
    }
}
