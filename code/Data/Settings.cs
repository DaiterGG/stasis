namespace Stasis.Data;
public class Settings
{
	public Settings()
	{
		MouseInvertX = false;
		MouseInvertY = false;
		Volume = 5;
		Sensitivity = 50;
		PitchAndRoll = true;
		CameraAngle = 4.5f;
	}
	public int Volume { get; set; }
	public int Sensitivity { get; set; }
	public bool MouseInvertX { get; set; }
	public bool MouseInvertY { get; set; }
	public bool PitchAndRoll { get; set; }
	public float CameraAngle {get; set;}
}
