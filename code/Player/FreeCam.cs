using Stasis.UI;
using System;

namespace Stasis.Player;
public sealed class FreeCam : Component
{
    [Property] public readonly GameObject thirdCam;
    readonly float force = 20f;
    float mult = 1f;
    Timer TIMER;
    MenuController MENUC;

    public void OnAwakeInit()
    {
        MENUC = Sng.Inst.MenuC;
        TIMER = Sng.Inst.Timer;
    }
    protected override void OnEnabled()
    {
        TIMER.StopTimer();
        WorldPosition = thirdCam.WorldPosition + new Vector3(0, 0, 10);
        WorldRotation = thirdCam.WorldRotation;
        Transform.ClearInterpolation();
    }
    protected override void OnUpdate()
    {
        if (!GameObject.Enabled) return;
        if (Input.Down("Up"))
        { //W or forward        
            WorldPosition += new Vector3(force * mult, 0, 0) * WorldRotation;
        }
        if (Input.Down("Down"))
        {
            WorldPosition += new Vector3(force * mult * -1, 0, 0) * WorldRotation;
        }
        if (Input.Down("Left"))
        {
            WorldPosition += new Vector3(0, force * mult, 0) * WorldRotation;
        }
        if (Input.Down("Right"))
        {
            WorldPosition += new Vector3(0, force * mult * -1, 0) * WorldRotation;
        }
        if (Input.Down("SelfDestruct"))
        {
            WorldPosition += new Vector3(0, 0, force * mult);
        }
        if (Input.Down("Crouch"))
        {
            WorldPosition += new Vector3(0, 0, force * mult * -1);
        }
        if (Input.Down("Sprint"))
        {
            mult = 3f;
        }
        else
        {
            mult = 1f;
        }
        var ee = WorldRotation.Angles();
        ee += Input.AnalogLook * Time.Delta * 3f;
        if (Math.Abs(ee.pitch) > 90) ee.pitch = 90 * Math.Sign(ee.pitch);
        ee.roll = 0;

        WorldRotation = ee.ToRotation();
        Transform.ClearInterpolation();
    }
}
