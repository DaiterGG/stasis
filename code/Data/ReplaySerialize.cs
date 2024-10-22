using System.Collections.Generic;
using System.Text.Json;
using System.IO;
using System.Numerics;
using System;

namespace Stasis.Data;

public class ReplaySerialize
{
    public static string TicksToStr(List<Tick> ticks)
    {
        string ticksUTF;
        using (var ms = new MemoryStream())
        using (var writer = new BinaryWriter(ms))
        {
            writer.Write(ticks.Count);
            foreach (var tick in ticks)
            {
                writer.Write(tick.Transform.x);
                writer.Write(tick.Transform.y);
                writer.Write(tick.Transform.z);
                writer.Write(tick.Pitch);
                writer.Write(tick.Yaw);
                writer.Write(tick.Roll);
                writer.Write(tick.Spin);
            }
            ticksUTF = Convert.ToBase64String(ms.ToArray());
        }
        return ticksUTF;
    }


    public static List<Tick> FromStrToTicks(string ticksUTF)
    {
        var ticks = new List<Tick>();
        var ticksBin = Convert.FromBase64String(ticksUTF);
        using (var ms = new MemoryStream(ticksBin))
        using (var reader = new BinaryReader(ms))
        {
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var transform = new Vector3(
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle());
                short pitch = reader.ReadInt16();
                short yaw = reader.ReadInt16();
                short roll = reader.ReadInt16();
                byte spin = reader.ReadByte();

                ticks.Add(new Tick(transform, pitch, yaw, roll, spin));
            }
        }
        return ticks;
    }
    public static Replay RemoveTicksArray(Replay r)
    {
        return new Replay(null, r.TicksUTF, r.Actions, r.StartTime, r.EndTime);
    }
    public static Replay JsonToReplay(Dictionary<string, object> d)
    {
        //Log.Info(d["Actions"]);
        Dictionary<int, List<Action>> acts;
        try
        {
            acts = JsonSerializer.Deserialize<Dictionary<int, List<Action>>>((JsonElement)d["Actions"]);
        }
        catch (Exception e)
        {
            acts = new Dictionary<int, List<Action>>();
            Sng.ELog(e, "err");
        }
        return new Replay(
                new List<Tick>(),
                JsonSerializer.Deserialize<string>((JsonElement)d["TicksUTF"]),
                acts,
                JsonSerializer.Deserialize<int>((JsonElement)d["StartTime"]),
                JsonSerializer.Deserialize<int>((JsonElement)d["EndTime"])
                );
    }
}
