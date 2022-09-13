using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

interface ISaveLoad
{
    object GetData();
}


[System.Serializable]
public class EntityData
{
    public string ResourcesPath;
    public virtual void OnLoad(GameObject gameObject) { }
}


public static class SaveAndLoad
{
    public static string fileName = "";

    [HideInInspector]
    public static ArrayList datas = new ArrayList();

    private static void updateFileName(ref string iFileName)
    {
        // CreateDirectory by default is readonly
        // But i can't manage to call the right using somehow...
        // TODO : Make the savefiles folder happen

        fileName = Application.persistentDataPath + "/" + /*Constants.FD_SAVEFILES +*/ iFileName;
    }

    public static bool save(string iFileName)
    {
        updateFileName(ref iFileName);

        if (File.Exists(fileName))
        { File.Delete(fileName); }
        FileStream fs = new FileStream(fileName, FileMode.Create);
        BinaryFormatter formatter = new BinaryFormatter();

        ArrayList save_datas = new ArrayList();
        foreach (object o in datas)
        {
            ISaveLoad saveable_o = (ISaveLoad)o;
            save_datas.Add(saveable_o.GetData());
        }

        try
        {
            formatter.Serialize(fs, save_datas);
        }
        catch (System.Runtime.Serialization.SerializationException e)
        {
            Debug.LogError("Failed to serialize : " + e.Message);
            return false;
        }
        finally
        {
            fs.Close();
        }

        save_datas.Clear();
        save_datas = null;
        formatter = null;
        fs = null;

        return true;
    }

    public static bool load(string iFileName)
    {
        updateFileName(ref iFileName);

        if (!File.Exists(fileName))
        { return false; }

        FileStream fs = new FileStream(fileName, FileMode.Open);
        ArrayList load_datas;

        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            load_datas = (ArrayList)formatter.Deserialize(fs);
            formatter = null;
        }
        catch (System.Runtime.Serialization.SerializationException e)
        {
            Debug.LogError("Failed to deserialize profile : " + e.Message);
            return false;
        }
        finally
        {
            fs.Close();
        }

        datas.Clear();
        foreach (object o in load_datas)
        {
            EntityData ed = (EntityData)o;
            ed.OnLoad(UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>(ed.ResourcesPath)));
        }

        load_datas.Clear();
        load_datas = null;
        fs = null;

        return true;
    }

    // TODO : Make me Generic
    public static bool loadGarageProfile(string iFileName, UIGarageProfile target)
    {
        updateFileName(ref iFileName);

        if (!File.Exists(fileName))
        { return false; }

        FileStream fs = new FileStream(fileName, FileMode.Open);
        ArrayList load_datas;

        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            load_datas = (ArrayList)formatter.Deserialize(fs);
            formatter = null;
        }
        catch (System.Runtime.Serialization.SerializationException e)
        {
            Debug.LogError("Failed to deserialize profile : " + e.Message);
            return false;
        }
        finally
        {
            fs.Close();
        }

        datas.Clear();
        foreach (object o in load_datas)
        {
            EntityData ed = (EntityData)o;
            ed.OnLoad(target.gameObject);
        }

        load_datas.Clear();
        load_datas = null;
        fs = null;

        return true;
    }

    public static bool loadGarageTest(string iFileName, UIGarageTestData target)
    {
        updateFileName(ref iFileName);

        if (!File.Exists(fileName))
        { return false; }

        FileStream fs = new FileStream(fileName, FileMode.Open);
        ArrayList load_datas;

        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            load_datas = (ArrayList)formatter.Deserialize(fs);
            formatter = null;
        }
        catch (System.Runtime.Serialization.SerializationException e)
        {
            Debug.LogError("Failed to deserialize profile : " + e.Message);
            return false;
        }
        finally
        {
            fs.Close();
        }

        datas.Clear();
        foreach (object o in load_datas)
        {
            EntityData ed = (EntityData)o;
            ed.OnLoad(target.gameObject);
        }

        load_datas.Clear();
        load_datas = null;
        fs = null;

        return true;
    }

}
