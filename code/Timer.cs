using System;
using Stasis.Data;
using Stasis.Player;
using Stasis.UI;
using Stasis.Zones;
namespace Stasis
{
    public class Timer
    {
        Sng SNG;
        RecordReplay RECORD;
        MenuController MENUC;
        EngineComponent ENGINE;
        SpinControl SPIN;
        ZoneControl ZONEC;
        public string TimerStr { get; private set; } = "Nan";
        public float timerSeconds = 0;
        public bool IsFinished { get; private set; } = false;
        public bool IsRunning { get; private set; } = false;
        public bool IsRequareReset { get; private set; }
        internal void OnAwakeInit()
        {
            SNG = Sng.Inst;
            RECORD = SNG.RecordC;
            MENUC = SNG.MenuC;
            ENGINE = SNG.Player.Engine;
            SPIN = SNG.Player.SpinC;
            ZONEC = SNG.ZoneC;
        }
        public void OnFixedGlobal()
        {
            //UpdateTimerStr();
            MENUC.IngameUI.Timer = SNG.FormatTime(timerSeconds) + TimerStr;
        }
        public void OnUpdateGlobal()
        {
            if (IsRunning) timerSeconds += Time.Delta;
        }
        void UpdateTimerStr()
        {
            //Engine starts at fixed update anyway
            if (ENGINE.isRunning && !IsFinished && !IsRequareReset && !IsRunning) TimerStart();
            if (IsFinished)
            {
                TimerStr = $" - Press \'{Input.GetButtonOrigin("Restart").ToUpper()}\' to start over";
            }
            else if (!IsRunning && !ENGINE.isRunning)
            {
                TimerStr = " - Timer starts when airborn";
            }
            else if (IsRequareReset)
            {
                TimerStr = $" - Press \'{Input.GetButtonOrigin("Restart").ToUpper()}\' to restart timer";
            }
            else if (!SPIN.IsAttached)
            {
                if (ZONEC.CheckPointActivated != null)
                    TimerStr = $" - Press \'{Input.GetButtonOrigin("SoftRestart").ToUpper()}\' to respawn";
                else TimerStr = $" - Press \'{Input.GetButtonOrigin("Restart").ToUpper()}\' to start over";
            }
            else
            {
                TimerStr = "";
            }
        }
        public void Reset()
        {
            TimerStr = "Nan";
            IsRunning = false;
            timerSeconds = 0;
            IsFinished = false;
            IsRequareReset = false;
            UpdateTimerStr();
        }

        public void TimerStart()
        {
            Sng.ELog("timer reStarted");
            IsRunning = true;
            RECORD.Start();
        }

        public void StopTimer()
        {
            if (!IsRunning) return;
            IsRunning = false;
            IsRequareReset = true;
            UpdateTimerStr();
        }
        public void TimerFinish()
        {
            IsFinished = true;
            IsRunning = false;
            UpdateTimerStr();
            RecordReplay.TimerStop();
        }
        /// <summary>
        /// Update when some actions may change Timer state
        /// Optimization instead of fixed update
        /// </summary>
        public void Update()
        {
            UpdateTimerStr();
        }

    }
}
