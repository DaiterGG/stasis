using System;
using Stasis.Player;
using Stasis.UI;
namespace Stasis
{
	public class Timer
	{
		Sng SNG;
		MenuController MENUC;
		EngineComponent ENGINE;
		public string TimerStr { get; private set; } = "Nan";
		public float timerSeconds = 0;

		public bool IsFinished { get; private set; } = false;
		public bool IsEngineRunning { get; private set; } = false;
		public bool IsRunning { get; private set; } = false;
		public bool IsRequareReset { get; private set; }
		internal void OnAwakeInit()
		{
			SNG = Sng.Inst;
			MENUC = SNG.MenuC;
			ENGINE = SNG.Player.Engine;
		}
		public void OnFixedGlobal()
		{
			UpdateTimer();
			MENUC.IngameUI.Timer = SNG.FormatTime( timerSeconds ) + TimerStr;
		}
		public void OnUpdateGlobal(){
			if ( IsRunning ) timerSeconds += Time.Delta;
		}
		public void UpdateTimer()
		{
			//Engine starts at fixed update anyway
			if ( ENGINE.isRunning && !IsFinished && !IsRequareReset) IsRunning = true;
			if ( IsFinished )
			{
				TimerStr =  $" - Press \'{Input.GetButtonOrigin( "Restart" ).ToUpper()}\' to start over";
			}
			else if ( !IsRunning && !ENGINE.isRunning )
			{
				TimerStr = " - Timer starts when airborn";
			}
			else if ( IsRequareReset )
			{
				TimerStr = $" - Press \'{Input.GetButtonOrigin( "Restart" ).ToUpper()}\' to restart timer";
			}
			else if ( ENGINE.isRunning && !ENGINE.inputActive )
			{
				TimerStr = $" - Press \'{Input.GetButtonOrigin( "Restart" ).ToUpper()}\' to start over";
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
			UpdateTimer();
		}

		public void StopTimer()
		{
			if ( !IsRunning ) return;
			IsRunning = false;
			IsRequareReset = true;
			UpdateTimer();
		}
		public void TimerFinish()
		{
			IsFinished = true;
			IsRunning = false;
			UpdateTimer();
		}
		public void UpdateEngine(bool eng){
			IsEngineRunning = eng;	
			UpdateTimer();
		}

	}
}
