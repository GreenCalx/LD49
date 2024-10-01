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
    public string activeProfile = "";
    private string fp_prefix = "";
    private readonly string fp_suffix = "GameProgressData.json";

    private GameProgressData gameProgressData;

    public Action onProfileLoaded;

    void OnDestroy()
    {
        Save();
    }

    public void init()
    {
        this.Log("init.");
        fp_prefix = Application.persistentDataPath + Constants.FD_SAVEFILES; // unauthorized in constructor so not const..
        if (!Directory.Exists(fp_prefix))
        {
            Directory.CreateDirectory(fp_prefix);
        }

        SwitchActiveProfile(PlayerPrefs.GetString("lastActiveProfile"));
    }

    public void Save()
    {
        if (HasActiveProfile()) SaveProfile(activeProfile);
    }

    public void Load()
    {
        if (HasActiveProfile()) LoadProfile(activeProfile);
    }

    public void ResetAndSave() {
        ResetAndSave(activeProfile);
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

    #region  Events API

    public bool IsUniqueEventDone(UniqueEvents.UEVENTS iEventID)
    {
        if (iEventID==UniqueEvents.UEVENTS.NONE) return true;

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
    #endregion

    #region  Profile API

    public string GetProfilePath(string profileName)
    {
        return Path.Combine(fp_prefix, profileName);
    }

    public string GetActiveProfilePath() => GetProfilePath(activeProfile);

    public void LoadProfile(string profileName)
    {
        string profilePath = Path.Combine(GetProfilePath(profileName), fp_suffix);
        try
        {
            using (StreamReader reader = new StreamReader(profilePath))
            {
                string dataToLoad = reader.ReadToEnd();
                gameProgressData = JsonUtility.FromJson<GameProgressData>(dataToLoad);
                onProfileLoaded?.Invoke();
            }
        } catch (IOException e)
        {
            this.LogError(e.ToString());
            this.LogError("Failed to locate/load fail for profile "+ activeProfile);
            return;

            // Do not try to save then load : whats the point?
            // if the profile is already the active one but not save yet, it is in loaded state
            // if the profile does not exists we dont want to pollute the user disk with data that we
            //    dont know if it will be used or not.
        }
    }

    public void SaveProfile(string profileName)
    {
        string profilePath = Path.Combine(GetProfilePath(profileName), fp_suffix);
        using (StreamWriter writer = new StreamWriter(profilePath))
        {
            string dataToWrite = JsonUtility.ToJson(gameProgressData);
            writer.Write(dataToWrite);
        }
    }

    public void ResetAndSave(string profileName)
    {
        LoadProfile(profileName);

        gameProgressData.uniqueEventsDone.Clear();

        SaveProfile(profileName);

        LoadProfile(activeProfile);
    }

    public List<string> GetAvailableProfileNames()
    {
        string[]     dirs   = Directory.GetDirectories(fp_prefix, "*", SearchOption.TopDirectoryOnly);
        List<string> retval = new List<string>(dirs.Length);

        foreach (string dir in dirs) {
            string profileName = dir.Replace(fp_prefix, "").Replace(fp_suffix, "");
            retval.Add(profileName);
        }

        return retval;
    }

    public void DeleteProfileIfExists(string iProfileName)
    {
        string path = fp_prefix + "/" + iProfileName;
        if (!Directory.Exists(path)) return;

        System.IO.Directory.Delete(path, true);

        // System.IO.DirectoryInfo di = new DirectoryInfo(path);
        // foreach (FileInfo file in di.GetFiles())
        // {
        //     file.Delete();
        // }
        // foreach (DirectoryInfo dir in di.GetDirectories())
        // {
        //     dir.Delete(true);
        // }

        // Directory.Delete(path);
    }

    public void CreateProfile(string profileName) {
        string profilePath = fp_prefix + profileName;
        if (!Directory.Exists(profilePath))
        {
            Directory.CreateDirectory(profilePath);
        } else {
            this.LogError("Profile with the name" + profileName + " already exists!");
        }
    }

    public void OverwriteProfile(string oldName, string newName) {
        // Erase fill if exists
        if (!string.IsNullOrEmpty(oldName)) DeleteProfileIfExists(oldName);
        if (!string.IsNullOrEmpty(newName)) CreateProfile        (newName);
    }

    public void SwitchActiveProfile(string profileName) {
        if (activeProfile == profileName) {
            this.LogWarn("profile " + profileName + " is already the active one.");
            return;
        }

        //NOTE: do not check for null or empty string => it might be what the user really wants to do!
        activeProfile = profileName;
        // but now check for it as the empty profile does not exists as a directory
        if (HasActiveProfile()) {
            ResetAndSave(activeProfile);
        }
    }

    public bool HasActiveProfile() {
        return !string.IsNullOrEmpty(activeProfile);
    }

    #endregion
}
