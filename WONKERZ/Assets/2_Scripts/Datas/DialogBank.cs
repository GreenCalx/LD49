using System.IO;
using System.Collections.Generic;
using UnityEngine;

public static class DialogBank
{
    [System.Serializable]
    public class DialogBankInfo
    {
        public NPCDialogBank[] npc_bank;

        public NPCDialogBank GetNPCBank(string iNPCName)
        {
            foreach (NPCDialogBank npc_dial in npc_bank)
            {
                if (npc_dial.name == iNPCName)
                    return npc_dial;
            }
            return null;
        }
    }
    
    [System.Serializable]
    public class NPCDialogBank
    {
        public string name;
        public NPCSceneBank[] scenes;

        public NPCSceneBank GetSceneBank(string iSceneName)
        {
            foreach (NPCSceneBank scene_bank in scenes)
            {
                if (scene_bank.name == iSceneName)
                    return scene_bank;
            }
            return null;
        }
    }

    [System.Serializable]
    public class NPCSceneBank
    {
        public string name;
        public NPCSceneDialog[] dialogs;
        public NPCSceneDialog GetDialog(int iID)
        {
            foreach (NPCSceneDialog scene_dialog in dialogs)
            {
                if (scene_dialog.id == iID)
                    return scene_dialog;
            }
            return null;
        }
    }

    [System.Serializable]
    public class NPCSceneDialog
    {
        public int id;
        public string[] dialog;
    }

    public static string playerName = "";

    public static DialogBankInfo bankInfo;

    public static void initBank(string iBankPath)
    {
        TextAsset dialogFile = Resources.Load(iBankPath) as TextAsset;
        
        if (!!dialogFile)
        {
            // Replace $player with actuel playername
            string json_str = dialogFile.text.Replace(Constants.TOK_PLAYER, playerName);

            bankInfo = JsonUtility.FromJson<DialogBankInfo>(json_str);
        }
            
    }

    public static string[] load(string iNPCName, string iSceneName, int iID)
    {
        if (bankInfo==null)
        {
            initBank(Constants.RES_DIALOGBANK);
        }

        NPCDialogBank npc_bank = bankInfo.GetNPCBank(iNPCName);
        if (null!=npc_bank)
        {
            NPCSceneBank scene_bank = npc_bank.GetSceneBank(iSceneName);
            if (null!=scene_bank)
            {
                NPCSceneDialog scene_dialog = scene_bank.GetDialog(iID);
                if (null!=scene_dialog)
                {
                    return scene_dialog.dialog;
                }
            }
        }
        return new string[]{};
    }

}
