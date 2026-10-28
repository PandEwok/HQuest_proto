using System;
using System.Globalization;
using System.IO;
using UnityEngine;

/// <summary>
/// Reads / writes saves on disk.
/// Folder: Application.persistentDataPath/Maps/map_slot_XX.json  (XX = slot number, starting at 01)
/// </summary>
public static class MapSaveSystem
{
    const string FolderName = "Maps";
    const string Prefix = "map_slot_";
    const string Extension = ".json";

    public static string FolderPath => Path.Combine(Application.persistentDataPath, FolderName);

    /// <param name="slot">slot index, starting at 0</param>
    public static string GetPath(int slot) => Path.Combine(FolderPath, $"{Prefix}{slot + 1:00}{Extension}");

    public static bool Exists(int slot) => File.Exists(GetPath(slot));

    public static bool TrySave(int slot, MapSaveData data, out string error)
    {
        error = null;
        try
        {
            Directory.CreateDirectory(FolderPath);
            string path = GetPath(slot);
            string tmp = path + ".tmp";

            File.WriteAllText(tmp, JsonUtility.ToJson(data, true));
            if (File.Exists(path)) File.Delete(path);
            File.Move(tmp, path);
            return true;
        }
        catch (Exception e)
        {
            error = e.Message;
            Debug.LogError($"[MapSaveSystem] Could not save (slot {slot + 1}) : {e}");
            return false;
        }
    }

    /// <summary>Returns null if the slot is empty or the file is unreadable.</summary>
    public static MapSaveData Load(int slot)
    {
        string path = GetPath(slot);
        if (!File.Exists(path)) return null;
        try
        {
            return JsonUtility.FromJson<MapSaveData>(File.ReadAllText(path));
        }
        catch (Exception e)
        {
            Debug.LogError($"[MapSaveSystem] Unreadable file (slot {slot + 1}) : {e}");
            return null;
        }
    }

    public static bool Delete(int slot)
    {
        try
        {
            string path = GetPath(slot);
            if (File.Exists(path)) File.Delete(path);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[MapSaveSystem] Could not delete (slot {slot + 1}) : {e}");
            return false;
        }
    }

    /// <summary>Index (starting at 0) of the highest slot that has a file, or -1 if there is none.</summary>
    public static int HighestExistingSlot()
    {
        int highest = -1;
        if (!Directory.Exists(FolderPath)) return highest;

        foreach (string file in Directory.GetFiles(FolderPath, Prefix + "*" + Extension))
        {
            string name = Path.GetFileNameWithoutExtension(file).Substring(Prefix.Length);
            if (int.TryParse(name, NumberStyles.Integer, CultureInfo.InvariantCulture, out int number))
            {
                highest = Mathf.Max(highest, number - 1);
            }
        }
        return highest;
    }
}