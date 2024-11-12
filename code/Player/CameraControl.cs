using System;
using Stasis.Data;
using Stasis.UI;
namespace Stasis.Player;
public sealed class CameraControl : Component
{
    EngineComponent ENGINE;
    FileControl FC;
    MenuController MENUC;
    Sng SNG;
    [Property] public FreeCam FreeCam;
    [Property] public List<GameObject> cameras;
    int iEnabled = 0;
    float cameraAngle;
    bool cameraAdjust;
    public void OnAwakeInit()
    {
        ENGINE = Sng.Inst.Player.Engine;
        FC = Sng.Inst.FileC;
        MENUC = Sng.Inst.MenuC;
        SNG = Sng.Inst;
        FreeCam.OnAwakeInit();
        cameraAdjust = false;
        cameraAngle = FC.Set.CameraAngle;
        UpdateAngle();
        UpdateCam();
    }
    /// <summary>
    /// Callen when IsGaming
    /// </summary>
    public void OnFixedGlobal()
    {
        if (Input.Pressed("CameraCycle"))
        {
            iEnabled += 1;
            if (iEnabled >= cameras.Count) iEnabled = 0;
            UpdateCam();
        }
        if (Input.Pressed("FreeCamera"))
        {
            FreeCamToggle();
        }
        if (Input.Pressed("attack2") && !FreeCam.GameObject.Enabled && iEnabled == 0)
        {
            cameraAdjust = true;
        }
        if (Input.Released("attack2"))
        {
            cameraAdjust = false;
            if (cameraAngle != FC.Set.CameraAngle)
            {
                FC.Set.CameraAngle = cameraAngle;
                FileControl.SaveSettings();
            }
        }
        if (SNG.GameState == GameState.ViewReplay && (Input.Pressed("Attack2") || Input.Pressed("Attack1")))
        {
            MENUC.ReplayUI.GameObject.Enabled = true;
        }

    }
    protected override void OnUpdate()
    {
        if (cameraAdjust)
        {
            cameraAngle = cameras[0].LocalRotation.Pitch();
            UpdateAngle();
        }
    }
    void UpdateAngle()
    {
        cameraAngle += Input.AnalogLook.pitch * Time.Delta * 3f;
        if (Math.Abs(cameraAngle) > 89) cameraAngle = 89 * Math.Sign(cameraAngle);
        cameras[0].LocalRotation = Rotation.From(cameraAngle, cameras[0].LocalRotation.Yaw(), 0);
    }
    public void UpdateCam()
    {
        var active = iEnabled;
        if (FreeCam.GameObject.Enabled)
        {
            active = -1;
        }
        for (int i = 0; i < cameras.Count; i++)
        {
            if (i == active) cameras[i].Enabled = true;
            else cameras[i].Enabled = false;
        }
    }
    public void FreeCamEnable(bool enable = true)
    {
        FreeCam.GameObject.Enabled = enable;
        UpdateCam();
        if (!enable && SNG.GameState == GameState.ViewReplay) MENUC.ReplayUI.GameObject.Enabled = true;
    }
    public void FreeCamToggle()
    {
        FreeCamEnable(!FreeCam.GameObject.Enabled);
    }
    public void TeleportToFreecam()
    {
        var zone = Sng.Inst.ZoneC;
        SaveStateControl.ApplySaveState(new SaveState()
        {
            Transform = FreeCam.WorldPosition,
            Rotation = FreeCam.WorldRotation,
            Velocity = new Vector3(0),
            AngularVelocity = new Vector3(0),
            Time = 0,
            SpinAttached = true,
            EngineRunning = true,
            Gain = 100,
            Progress = 100,
            ActiveBeacons = zone.GetActiveBeacons(),
            CheckPointActivated = zone.CheckPointActivated
        });

        FreeCamEnable(false);

    }
}
