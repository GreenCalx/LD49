using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

using Schnibble;

/*******************************************************************
 *   
 *    ______________________________________________________________
 *   |
 *   |______________________________________________________________
 *
 ********************************************************************/
public class GameProgressSaveManager : MonoBehaviour
{
    [System.Serializable]
    public class GameProgressData
    {
        public List<UniqueEvents.UEVENTS> uniqueEventsDone = new List<UniqueEvents.UEVENTS>();
    }

    // Profile used to save 'user' specific GameProgressDatas
    // TODO Merge savefiles
    // TODO when all type of saves are merged, implement the logic around the profile.
    public string activeProfile = "foobar";
    private string fp_prefix = "";
    private readonly string fp_suffix = "GameProgressData.json";
    
    private GameProgressData gameProgressData;
    private bool errorAtLoad = false;

    public string profileDataFilePath;

    public void init()
    {
        this.Log("init.");
        fp_prefix = Application.persistentDataPath + Constants.FD_SAVEFILES; // unauthorized in constructor so not const..
        if (!Directory.Exists(fp_prefix))
        {
            Directory.CreateDirectory(fp_prefix);
        }

        updateFilePath();
    }

    public void updateFilePath()
    {
        profileDataFilePath = Path.Combine(fp_prefix, activeProfile);
    }

    public void Load()
    {
        updateFilePath();
        string saveProgPath = Path.Combine(profileDataFilePath, fp_suffix);
        try
        {
            using (StreamReader reader = new StreamReader(saveProgPath))
            {
                string dataToLoad = reader.ReadToEnd();
                gameProgressData = JsonUtility.FromJson<GameProgressData>(dataToLoad);
            }
        } catch (IOException e)
        {
            // no file found, try to create a new one then call again the Load(). If it fails twice we give up.
            if (errorAtLoad)
            {
                this.LogError("GameProgressSaveManager::Failed to locate/load fail for profile "+ activeProfile);
                return;
            }
            
            Save(); // will create a file
            errorAtLoad = true;
            Load();
        }

    }

    public void Save()
    {
        updateFilePath();
        string saveProgPath = Path.Combine(profileDataFilePath, fp_suffix);
        using (StreamWriter writer = new StreamWriter(saveProgPath))
        {
            string dataToWrite = JsonUtility.ToJson(gameProgressData);
            writer.Write(dataToWrite);
        }
    }

    public void ResetAndSave()
    {
        gameProgressData.uniqueEventsDone.Clear();
        Save();
    }

    public GameProgressData GetGameProgressData()
    {
        return gameProgressData;
    }

    public GameProgressSaveManager()
    {
        gameProgressData = new GameProgressData();
        gameProgressData.uniqueEventsDone = new List<UniqueEvents.UEVENTS>();
    }

    public bool IsUniqueEventDone(UniqueEvents.UEVENTS iEventID)
    {
        if (iEventID==UniqueEvents.UEVENTS.NONE)
        return true;
        return gameProgressData.uniqueEventsDone.Contains(iEventID);
    }

    // >> SET specific game data of GameProgressData
    public void notifyUniqueEventDone(UniqueEvents.UEVENTS iEventID)
    {
        if (iEventID==UniqueEvents.UEVENTS.NONE)
        return;

        if (gameProgressData == null)
        gameProgressData = new GameProgressData();

        if (!gameProgressData.uniqueEventsDone.Contains(iEventID))
        {
            gameProgressData.uniqueEventsDone.Add(iEventID);
            Save();
        }
    }

    public List<string> GetAvailableProfileNames()
    {
        List<string> retval = new List<string>();

        string[] dirs = Directory.GetDirectories(fp_prefix, "*", SearchOption.TopDirectoryOnly);
        foreach (string dir in dirs) {
            string p_name = dir.Replace(fp_prefix, "");
            p_name = p_name.Replace(fp_suffix, "");
            this.Log(p_name);
            retval.Add(p_name);
        }
        return retval;
    }

    public void DeleteProfileIfExists(string iProfileName)
    {
        string path = fp_prefix + "/" + iProfileName;
        if (!Directory.Exists(path))
        return;

        System.IO.DirectoryInfo di = new DirectoryInfo(path);
        foreach (FileInfo file in di.GetFiles())
        {
            file.Delete(); 
        }
        foreach (DirectoryInfo dir in di.GetDirectories())
        {
            dir.Delete(true); 
        }

        Directory.Delete(path);
    }

    public void CreateActiveProfile()
    {
        string dir_path = fp_prefix + activeProfile;
        if (!Directory.Exists(dir_path))
        {
            Directory.CreateDirectory(dir_path);
        }
    }
}
