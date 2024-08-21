namespace Sandbox
{
	public class MainTimer
	{
		Sng SNG;
		EngineComponent ENGINE;
		public MainTimer()
		{
			SNG = Sng.Inst;
			ENGINE = SNG.Player.Engine;
		}
		public string TimerStr { get; private set; } = "Nan";
		public float timerSeconds = 0;

		public bool IsRunning { get; set; } = false;
		public bool IsFinished { get; set; } = false;
		public bool IsRequareReset { get; set; } = false;
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
