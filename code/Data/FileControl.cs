using System.Text.RegularExpressions;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Sandbox.Utility;
namespace Stasis.Data;
public sealed class FileControl
{
    public Settings Set { get; set; }
    public List<MapData> Maps { get; set; } = new List<MapData>();
    public MapData currentMap;
    static string s = "Settings.json";
    static string m = "Maps.json";
    Sng SNG;
    Timer TIMER;
    RecordReplay RECORD;
    public string[] FeaturedMaps;
    public string[] OfficialMaps;
    public void OnAwakeInit()
    {
        SNG = Sng.Inst;
        TIMER = SNG.Timer;
        RECORD = SNG.RecordC;
        FeaturedMaps = new string[] { };
        OfficialMaps = new string[]
        {
        "move.plground2",
        "move.hexagon3",
        "move.sharpv3",
        "move.zonetrial",
        };

        FilesInit();
    }
    public void OnStartInit()
    {
        AddOfficialMaps();
    }

    public void FilesInit()
    {
        Set = new Settings();
        if (FileSystem.Data.FileExists(s))
        {
            try
            {
                Set = FileSystem.Data.ReadJson<Settings>(s);
            }
            catch (Exception err) { Log.Error(err.Message); }
        }
        else
        {
            Set = new Settings();
            FileSystem.Data.WriteAllText(s, ObjToJson(Set));
        }
        if (FileSystem.Data.FileExists(m))
        {
            try
            {
                Maps = JsonSerializer.Deserialize<List<MapData>>(FileSystem.Data.ReadAllText(m), new JsonSerializerOptions { IncludeFields = true });
            }
            catch (Exception err) { Log.Error(err.Message); }
        }
        else
        {
            try
            {
                Maps = new List<MapData>();
                FileSystem.Data.WriteAllText(m, ObjToJson(Maps));

            }
            catch (Exception err) { Log.Warning("Move/Stasis folder does not exist?" + err.Message); }
        }
    }
    public void AddOfficialMaps()
    {
        //demote updated maps
        foreach (var i in Maps)
        {
            if (i.Type == "official")
            {
                if (!OfficialMaps.Contains(i.Indent))
                {
                    i.Type = "community";
                    SaveMaps();
                }
                else if (FeaturedMaps.Contains(i.Indent))
                {
                    i.Type = "featured";
                    SaveMaps();
                }
            }
            else
            if (i.Type == "featured")
            {
                if (!FeaturedMaps.Contains(i.Indent))
                {
                    i.Type = "community";
                    SaveMaps();
                }
                else if (OfficialMaps.Contains(i.Indent))
                {
                    i.Type = "official";
                    SaveMaps();
                }

            }
            else if (i.Type == "community")
            {
                if (FeaturedMaps.Contains(i.Indent))
                {
                    i.Type = "featured";
                    SaveMaps();
                }
                else if (OfficialMaps.Contains(i.Indent))
                {
                    i.Type = "official";
                    SaveMaps();
                }
            }

        }
        foreach (var i in FeaturedMaps)
        {
            FetchNewMap(i, "featured");
        }
        foreach (var i in OfficialMaps)
        {
            FetchNewMap(i, "official");
        }
    }
    static string CaseInsenstiveReplace(string originalString, string oldValue, string newValue)
    {
        Regex regEx = new Regex(oldValue,
        RegexOptions.IgnoreCase | RegexOptions.Multiline);
        return regEx.Replace(originalString, newValue);
    }
    public static void SaveMaps()
    {
        FileSystem.Data.WriteAllText(m, ObjToJson(Sng.Inst.FileC.Maps).Replace(@"\u002B", "+"));

        Sng.Inst.MenuC.UpdateMapsList();
    }
    public static void SaveSettings()
    {
        FileSystem.Data.WriteAllText(s, ObjToJson(Sng.Inst.FileC.Set));
    }
    private static string ObjToJson(object o)
    {
        return JsonSerializer.Serialize(o, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
    public void FetchNewMap(string indent, string type)
    {
        var found = Maps.FirstOrDefault(x =>
        {
            return x.Indent == indent;
        });
        if (found != default(MapData) && found != null && (found.Img == null || found.Name == null || found.Description == null))
        {
            found = default;
        }
        var _new = false;
        if (found == default(MapData) || found == null)
        {
            found = new MapData { Type = type };
            _new = true;
        }
        try
        {
            FetchMap(indent, found).Wait();
        }
        catch (Exception e) { Log.Info("Fetching map failed, are you offline? " + e.Message); }

        if (_new) Maps.Add(found);


        SaveMaps();
    }
    static public async Task FetchMap(string packageIndent, MapData mapData)
    {
        var package = await Package.Fetch(packageIndent, true);
        if (package == null) throw new Exception("Fetching failled");
        mapData.Name = package.Title;
        mapData.Description = package.Summary;
        mapData.Indent = package.FullIdent;
        mapData.Img = package.Thumb;
    }
    public void DownloadAndLoad(string packageIndent)
    {
        SetCurrentMap(packageIndent);
        try
        {
            DownloadScene(packageIndent).Wait();
        }
        catch (Exception e) { Log.Info("Download failed, try again" + e); }
        if (tempFile.ResourceName == null)
            Log.Info("Map Name not found");
        SNG.LoadNewMap(tempFile);
    }
    static SceneFile tempFile { get; set; } = new SceneFile();
    public async Task DownloadScene(string sceneIndent)
    {
        var package = await Package.Fetch(sceneIndent, false);

        var meta = package.GetMeta("PrimaryAsset", "ERROR");
        var g = await package.MountAsync();

        //tempFile = package.GetMeta<SceneFile>( "PrimaryAsset" );

        tempFile.LoadFromJson(g.ReadAllText(meta));
        //Sng.Inst.Scene.Load( scene );
    }
    public void SetCurrentMap(string ind)
    {
        currentMap = Maps.FirstOrDefault(map =>
        {
            return map.Indent == ind;
        });
        if (currentMap == default(MapData) || currentMap == null)
        {
            Log.Warning("Fetched data Don't have that map");
            return;
        }
    }

    public void InfoSerialize(Info info)
    {
        if (info == null)
        {
            currentMap = null;
            return;
        }
        if (currentMap == null)
        {
            Log.Warning("Map data was not fetched correctly");
            return;
        }
        try
        {

            currentMap.Version = info.Version;
            currentMap.Author = info.Author;
            currentMap.DifficultyTier = info.DifficultyTier;
            currentMap.SpeedRun = info.SpeerunMap;
            currentMap.GoldTime = info.SpeerunMap ? info.GoldTime : 0;
            currentMap.SilverTime = info.SpeerunMap ? info.SilverTime : 0;
            currentMap.BronzeTime = info.SpeerunMap ? info.BronzeTime : 0;

        }
        catch (Exception err) { Log.Warning("Map serialize error: " + err.Message); }

        SaveMaps();
    }
    public void SetScore()
    {
        if (currentMap == null) return;
        var scr = new Score()
        {
            Time = TIMER.timerSeconds,
            Date = DateTime.Now,
            DisplayName = Steam.PersonaName,
            Replay = RECORD.TryToGet(),
        };
        //if (!Game.IsEditor)
        LBControl.SetScore(scr, currentMap.Indent);

        currentMap.Scores.Add(scr);
        currentMap.Scores.Sort((x, y) => x.Time.CompareTo(y.Time));
        SaveMaps();
    }
    public List<Score>? GetScores(string indent)
    {
        foreach (var map in Maps)
        {
            if (map.Indent == indent)
            {
                return map.Scores;
            }
        }
        return null;
    }
}
