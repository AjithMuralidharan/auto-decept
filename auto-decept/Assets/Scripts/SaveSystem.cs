using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//JSON persistance. No warnings, just return failures as false
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
    private static readonly string FileName = "save.json";
    public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    public static bool Save(SaveData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, false);
            File.WriteAllText(FilePath, json);
            Debug.Log("[SaveSystem] Wrote file: " + FilePath + " (chars=" + json.Length + ")");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("[SaveSystem] Save failed: " + e);
            return false;
        }
    }

    public static bool TryLoad(out SaveData data)
    {
        data = null;
        try
        {
            if (!File.Exists(FilePath))
            {
                Debug.Log("[SaveSystem] No file at: " + FilePath);
                return false;
            }

            string json = File.ReadAllText(FilePath);
            data = JsonUtility.FromJson<SaveData>(json);
            if (data == null)
            {
                Debug.LogWarning("[SaveSystem] JSON parsed null at: " + FilePath);
                return false;
            }

            Debug.Log("[SaveSystem] Loaded file: " + FilePath + " (chars=" + json.Length + ")");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("[SaveSystem] Load failed: " + e);
            data = null;
            return false;
        }
    }

    public static void Delete()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
                Debug.Log("[SaveSystem] Deleted file: " + FilePath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[SaveSystem] Delete failed: " + e);
        }
    }
}