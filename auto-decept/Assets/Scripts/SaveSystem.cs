using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//JSON persistance. No warnings; just return failures as false
[Serializable]
public class SaveData
{
    public int layoutIndex;
    public int score;
    public int combo;
    public int seed;
    public List<int> deckIds = new List<int>();
    public List<int> matchedIds = new List<int>();
}

public static class SaveSystem
{
    private static string FilePath
    {
        get { return Path.Combine(Application.persistentDataPath, "save.json"); }

    }

    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, false);
        File.WriteAllText(FilePath, json);
    }

    public static bool TryLoad(out SaveData data)
    {
        data = null;
        if (File.Exists(FilePath)) return false;

        try
        {
            string json = File.ReadAllText(FilePath);
            data = JsonUtility.FromJson<SaveData>(json);
            return data != null;
        }

        catch
        {
            data = null;
            return false;
        }
    }

    public static void Delete()
    {
        if (File.Exists(FilePath)) File.Delete(FilePath);
    }
}