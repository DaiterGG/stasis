using System;

namespace Sandbox.Data;
public class Score
{
	public Score( float time, DateTime date )
	{
		Time = time;
		Date = date;
	}
	public float Time { get; set; }
	public DateTime Date { get; set; }
}
