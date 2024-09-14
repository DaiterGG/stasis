using System;
using Stasis.Data;
using Stasis.Player;
using Stasis.UI;
using Stasis.Zones;
namespace Stasis;
public sealed class Sng : Component
{
	private static Sng _sng;
	public static Sng Inst { get { return _sng; } }

	[Property] public MenuController MenuC;
	[Property] public PlayerComp Player;
	[Property] public ZoneControl ZoneC;
	public RecordReplay RecordC = new();
	public ViewReplay ViewReplayC = new();
	public LBControl LB = new();
	public FileControl FileC = new();
	public Timer Timer = new();
	public SaveStateControl StateC = new();
	public bool firstTime { get; set; } = true;
	public bool blurOn { get; set; }
	protected override void OnAwake()
	{
		if (Game.IsEditor && Scene.Name != "Scene")
		{
			_sng = this;
			ELog("testing in editor");
		}
		else if (GameObject.Parent.Name == "Menu")
		{
			_sng = this;
		}
		else
		{
			GameObject.Parent.Enabled = false;
			GameObject.Parent.DestroyImmediate();
			return;
		}
		blurOn = !Game.IsEditor;
		OnAwakeInit();
	}
	public void OnAwakeInit()
	{
		Timer.OnAwakeInit();
		StateC.OnAwakeInit();
		FileC.OnAwakeInit();
		MenuC.OnAwakeInit();
		Player.Engine.OnAwakeInit();
		Player.SoundC.OnAwakeInit();
		Player.SpinC.OnAwakeInit();
		ZoneC.OnAwakeInit();
		Player.CameraC.OnAwakeInit();
	}

	protected override void OnStart()
	{
		FileC.AddOfficialMaps();
		//File.FetchNewMap( "move.stasis_playgr", "official" );
		//File.FetchNewMap( "dicta.base", "community" );
		//File.DownloadAndLoad( "move.stasis_playground", true );

		if (Game.IsEditor && Scene.Name != "Scene")
		{
			FileC.SetCurrentMap("move.plground");
			MapInit();
		}
		else
		{
			FileC.DownloadAndLoad("move.plground");
		}

		base.OnStart();
	}
	/// <summary>
	/// Called on every map load
	/// </summary>
	private void MapInit()
	{
		Info MapInfo = null;

		ZoneC.ZonesClearAll();
		var heap = Scene.GetAllObjects(true);
		foreach (var obj in heap)
		{
			if (obj.Name.ToLower().Contains("spawn") && obj.Name.ToLower().Contains("player"))
			{
				ZoneC.SpawnPoint = obj.Transform;
			}
			else if (obj.Name.ToLower().Contains("start") && obj.Name.ToLower().Contains("player"))
			{
				ZoneC.StartPoint = obj.Transform;
			}
			else if (obj.Name == "Map Info")
			{
				MapInfo = obj.Components.Get<Info>();
				if (MapInfo == null)
					ELog("no info component found");
				else
					ELog("Info Found");
			}
			else
			{
				var zone = obj.Components.Get<IZone>();
				if (zone != null)
					ZoneC.Zones.Add(zone);
			}
		}
		if (MapInfo == null) Log.Warning("Info Not Found");

		FileC.InfoSerialize(MapInfo);

		if (ZoneC.Zones.Count == 0) Log.Warning("No Zones Found");
		else ZoneC.ZoneInit();

		if (ZoneC.StartPoint == null) Log.Warning("Start Not Found");
		Spawn();
	}
	protected override void OnFixedUpdate()
	{
		OnFixedUpdateSequence();
	}
	private void OnFixedUpdateSequence()
	{
		try
		{
			MenuC.BlackUI.Opacity -= 0.005f;
			if (MenuC.IsGaming)
			{
				if (Input.Pressed("SoftRestart"))
				{
					if (!ZoneC.TrySoftRestart())
						Play();
				}
				if (Input.Pressed("Restart"))
				{
					Play();
				}
				if (Input.Pressed("SelfDestruct"))
				{
					if (Player.CameraC.FreeCam.GameObject.Enabled)
					{
						Player.CameraC.TeleportToFreecam();
					}
					else
					{
						Player.JumpC.TryToJump();
						Player.SpinC.SpinCollision();
					}
				}
				if (Input.Pressed("Back") || Input.EscapePressed)
				{
					MenuC.OpenMenu();
				}
				if (Input.Pressed("HideUI"))
				{
					MenuC.InGameUIToggle();
				}
				if (Input.Pressed("ShowInfo"))
				{
					MenuC.InGameHelpToggle();
				}
			}
			else
			{
				if (Input.Pressed("SelfDestruct"))
				{
					MenuC.Play();
				}
				if (Input.Pressed("Quit"))
				{
					MenuC.Quit();
				}
			}

			StateC.OnFixedGlobal();
			Player.SoundC.OnFixedGlobal();
			Player.SpinC.OnFixedGlobal();
			Player.Engine.OnFixedGlobal();
			if (MenuC.IsGaming) Timer.OnFixedGlobal();
			Player.CameraC.OnFixedGlobal();
		}
		catch (Exception e) { LogOnce("Main fixed error" + e.StackTrace); }
	}
	protected override void OnUpdate()
	{
		Timer.OnUpdateGlobal();
		StateC.OnUpdateGlobal();
	}
	public void LoadNewMap(SceneFile file)
	{

		Log.Info("Map trying to load: " + file.ResourceName);
		try
		{
			Scene.Load(file);
		}
		catch (Exception e) { Log.Warning("Map not found localy: " + e.Message); }

		MapInit();
		MenuC.OpenMenu();
	}
	// TODO: Make more states like Reset/Play/Spawn
	/// <summary>
	/// On map change and on back to menu
	/// </summary>
	public void Spawn()
	{
		if (ZoneC.SpawnPoint == null)
		{
			Log.Warning("No spawn and no start point");
			ZoneC.SpawnPoint = Player.Transform;
			ZoneC.StartPoint = Player.Transform;
		}
		Player.Transform.Position = ZoneC.SpawnPoint.Position;
		Player.Transform.Rotation = ZoneC.SpawnPoint.Rotation;
		Player.Transform.ClearInterpolation();
		Player.Engine.ResetPos(true);
		MenuC.SetCameraLook();
		StateC.Enabled = false;
		Player.SpinC.RestartSpin();
	}
	/// <summary>
	/// On play button and reset button
	/// </summary>
	public void Play()
	{
		ZoneC.ZonesReset();
		TeleportPlayer(ZoneC.StartPoint);
		Timer.Reset();
		Player.CameraC.FreeCamEnable(false);
		Player.SpinC.RestartSpin();
		StateC.Reset();
	}
	public void TeleportPlayer(GameTransform pos)
	{
		if (ZoneC.SpawnPoint == null)
		{
			Log.Warning("No spawn and no start point");
			ZoneC.SpawnPoint = Player.Transform;
			ZoneC.StartPoint = Player.Transform;
		}
		if (pos == null)
		{
			Player.Transform.Position = ZoneC.SpawnPoint.Position;
			Player.Transform.Rotation = ZoneC.SpawnPoint.Rotation;
		}
		else
		{
			Player.Transform.Position = pos.Position;
			Player.Transform.Rotation = pos.Rotation;
		}
		Player.Transform.ClearInterpolation();
		Player.Engine.ResetPos(false);
	}

	public string FormatTime(float totalSeconds)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(totalSeconds);

		string formattedTime = "";

		if (timeSpan.Hours > 0)
		{
			formattedTime += $"{timeSpan.Hours:D2}:";
		}
		else if (timeSpan.Minutes > 0 || timeSpan.Hours > 0)
		{
			formattedTime += $"{timeSpan.Minutes:D2}:";
		}
		formattedTime += $"{timeSpan.Seconds:D2}.{timeSpan.Milliseconds:D3}";

		if (formattedTime[0] == '0' && formattedTime[1] != '.')
			formattedTime = formattedTime.Substring(1);

		return formattedTime;
	}
	//unused
	private void FindMaps()
	{
		var maps = FileSystem.Mounted.FindFile("/", "*.vpk");
		foreach (var map in maps)
		{
			//	Log.Info( map );
		}

	}
	/// <summary>
	/// Log. for Editor Mode only
	/// type = "Log", "Warn", "Err"
	/// </summary>
	public static void ELog(object s, string type = "Log")
	{
		if (Game.IsEditor)
		{
			if (type == "Log")
				Log.Info(s);
			else if (type == "Warn")
				Log.Warning(s);
			else if (type == "Err")
				Log.Error(s);
		}
	}
	string lastLog;
	public static void LogOnce(object s, bool editLog = true)
	{
		if (Inst.lastLog != s.ToString())
		{
			if (editLog)
			{
				ELog(s);
			}
			else Log.Info(s);
			Inst.lastLog = s.ToString();
		}
	}


}

