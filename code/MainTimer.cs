﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	public class MainTimer
	{

		public string TimerStr {  get; private set; } = "Nan";
		private float timerSeconds = 0;

		public bool IsRunning { get; set; } = false;
		public bool IsFinished { get; set; } = false;
		public bool IsRequareReset { get; set; } = false;
		public bool IsAirborne { get; set; } = false;
		public void UpdateTimer()
		{
			if(IsRunning) timerSeconds += Time.Delta;
			if ( IsFinished )
			{
				TimerStr = GetStringFromTime() + $" - Press '{Input.GetButtonOrigin("Restart").ToUpper()} to start over";
			} else if (!IsRunning && !IsAirborne )
			{
				TimerStr = "Timer starts when airborn";
			} else if (IsRequareReset)
			{
				TimerStr = $"Press '{Input.GetButtonOrigin("Restart").ToUpper()}' to restart timer";
			} else
			{
				TimerStr = GetStringFromTime();
			}
		}
		private string GetStringFromTime()
		{
			TimeSpan timeSpan = TimeSpan.FromSeconds(timerSeconds);
			int milliseconds = (int)(timeSpan.Milliseconds / 10);
			return $"{timeSpan.Minutes}.{timeSpan.Seconds:D2}.{timeSpan.Milliseconds:D2}";
		}
		public void TimerReset()
		{
			timerSeconds = 0;
			IsFinished = false;
		}
	}
}