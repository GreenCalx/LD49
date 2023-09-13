using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

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
        public List<string> uniqueEventsDone = new List<string>();
    }

    // Profile used to save 'user' specific GameProgressDatas
    // TODO Merge savefiles
    // TODO when all type of saves are merged, implement the logic around the profile.
    public string activeProfile = "foobar";
    
    private GameProgressData gameProgressData;
    private bool errorAtLoad = false;

    private string dataFilePath;
    void Awake()
    {
        string filename = activeProfile + "GameProgressData.json";
        dataFilePath = Path.Combine(Application.persistentDataPath, filename);
    }

    public void Load()
    {
        try
        {
            using (StreamReader reader = new StreamReader(dataFilePath))
            {
                string dataToLoad = reader.ReadToEnd();
                gameProgressData = JsonUtility.FromJson<GameProgressData>(dataToLoad);
            }
        } catch (IOException e)
        {
            // no file found, try to create a new one then call again the Load(). If it fails twice we give up.
            if (errorAtLoad)
            {
                Debug.LogError("GameProgressSaveManager::Failed to locate/load fail for profile "+ activeProfile);
                return;
            }
            
            Save(); // will create a file
            errorAtLoad = true;
            Load();
        }

    }

    public void Save()
    {
        using (StreamWriter writer = new StreamWriter(dataFilePath))
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
        gameProgressData.uniqueEventsDone = new List<string>();
    }

    public bool IsUniqueEventDone(string iEventID)
    {
        return gameProgressData.uniqueEventsDone.Contains(iEventID);
    }

    // >> SET specific game data of GameProgressData
     public void notifyUniqueEventDone(string iEventID)
     {
         if (gameProgressData == null)
            gameProgressData = new GameProgressData();

        if (!gameProgressData.uniqueEventsDone.Contains(iEventID))
            gameProgressData.uniqueEventsDone.Add(iEventID);

        Save();
     }
}