namespace Sandbox.Data;
public class Settings
{
	public Settings()
	{
		MouseInvertX = false;
		MouseInvertY = false;
		Volume = 5;
		Sensitivity = 50;
	}
	public int Volume { get; set; }
	public int Sensitivity { get; set; }
	public bool MouseInvertX { get; set; }
	public bool MouseInvertY { get; set; }
}
