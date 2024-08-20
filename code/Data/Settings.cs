﻿namespace Sandbox.Data;
public class Settings
{
	public Settings()
	{
		MouseInvertX = false;
		MouseInvertY = false;
	}
	public bool MouseInvertX { get; set; } = false;
	public bool MouseInvertY { get; set; } = false;
}
