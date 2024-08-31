using Stasis.Player;
using Stasis.UI;
namespace Stasis
{
	public class Timer
	{
		Sng SNG;
		MenuController MENUC;
		EngineComponent ENGINE;
		public Timer()
		{
			SNG = Sng.Inst;
			MENUC = SNG.MenuC;
			ENGINE = SNG.Player.Engine;
		}
		public string TimerStr { get; private set; } = "Nan";
		public float timerSeconds = 0;

		public bool IsFinished { get; private set; } = false;
		bool IsRunning { get; set; } = false;
		public bool IsRequareReset { get; set; }
		public void OnFixedGlobal()
		{
			UpdateTimer();
			MENUC.IngameUI.Timer = TimerStr;
		}
		public void UpdateTimer()
		{
			if ( ENGINE.isRunning && !IsFinished ) IsRunning = true;
			if ( IsFinished )
			{
				TimerStr = SNG.FormatTime( timerSeconds ) + $"\n - Press '{Input.GetButtonOrigin( "Restart" ).ToUpper()}\' to start over";
			}
			else if ( !IsRunning && !ENGINE.isRunning )
			{
				TimerStr = "Timer starts when airborn";
			}
			else if ( IsRequareReset )
			{
				TimerStr = $"Press '{Input.GetButtonOrigin( "Restart" ).ToUpper()}' to restart timer";
			}
			else if ( ENGINE.isRunning && !ENGINE.inputActive )
			{
				timerSeconds += Time.Delta;
				TimerStr = $"{SNG.FormatTime( timerSeconds )}\n - Press '{Input.GetButtonOrigin( "Restart" ).ToUpper()}\' to start over";
			}
			else
			{
				timerSeconds += Time.Delta;
				TimerStr = SNG.FormatTime( timerSeconds );
			}
		}
		public void Reset()
		{
			TimerStr = "Nan";
			IsRunning = false;
			timerSeconds = 0;
			IsFinished = false;
			IsRequareReset = false;
		}

		public void StopTimer()
		{
			IsRunning = false;
			IsRequareReset = true;
		}
		public void TimerFinish()
		{
			IsFinished = true;
		}
	}
}
