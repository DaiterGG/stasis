using System;

namespace Sandbox
{
	public class MainTimer
	{
		EngineComponent ENGINE;
		public MainTimer()
		{
			ENGINE = Sng.Inst.Player.Engine;
		}
		public string TimerStr { get; private set; } = "Nan";
		private float timerSeconds = 0;

		public bool IsRunning { get; set; } = false;
		public bool IsFinished { get; set; } = false;
		public bool IsRequareReset { get; set; } = false;
		public void UpdateTimer()
		{
			if ( ENGINE.isRunning && !IsFinished ) IsRunning = true;
			if ( IsFinished )
			{
				TimerStr = GetStringFromTime() + $"\n - Press '{Input.GetButtonOrigin( "Restart" ).ToUpper()}\' to start over";
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
				TimerStr = $"{GetStringFromTime()}\n - Press '{Input.GetButtonOrigin( "Restart" ).ToUpper()}\' to start over";
			}
			else
			{
				timerSeconds += Time.Delta;
				TimerStr = GetStringFromTime();
			}
		}
		private string GetStringFromTime()
		{
			TimeSpan timeSpan = TimeSpan.FromSeconds( timerSeconds );
			int milliseconds = (int)(timeSpan.Milliseconds / 10);
			return $"{timeSpan.Minutes}.{timeSpan.Seconds:D2}.{timeSpan.Milliseconds:D2}";
		}
		public void TimerReset()
		{
			timerSeconds = 0;
			IsFinished = false;
			IsRequareReset = false;
		}

		public void StopTimer()
		{
			IsRunning = false;
			IsRequareReset = true;
		}
	}
}
