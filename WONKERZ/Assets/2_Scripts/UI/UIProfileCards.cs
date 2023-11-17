using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;
using System;

public class UIProfileCards : UIPanelTabbed
{
    public enum FD_MODE {READ, WRITE};
    public FD_MODE mode;
    public List<UIProfileCardPanel> profileCards;
    public string newProfileName = "FOOBAR";

    public void updateNewProfileName(string iS)
    {
        newProfileName = iS;
    }
    // Start is called before the first frame update
    void Start()
    {
        self_init();
    }

    public void self_init()
    {
        profileCards = new List<UIProfileCardPanel>(tabs.Count);
        foreach(UITab t in tabs)
        {
            UIProfileCardPanel uipcp = (UIProfileCardPanel) t;
            if (uipcp==null)
            {
                Debug.LogError("A card profile doesn't carry UIProfileCardPanel : " + t.gameObject.name);
                continue;
            }
            profileCards.Add(uipcp);
        }

        List<string> profiles = Access.GameProgressSaveManager().GetAvailableProfileNames();
        for (int i=0; i < profiles.Count; i++)
        {
            if (i >= profileCards.Count)
                break;
            profileCards[i].profileName.text = profiles[i];
        }
    }

    public void activateSelectedCard()
    {
        UIProfileCardPanel uipcp = (UIProfileCardPanel) tabs[selected];
        if (uipcp==null)
        {
            Debug.LogError("Selected Card is not activable");
            return;
        }
        string selected_profile_name = uipcp.profileName.text;

        if (mode==FD_MODE.WRITE)
        {
            OverWriteProfile(selected_profile_name, newProfileName);
        } else {
            LoadProfile(selected_profile_name);
        }
        
    }

    public void LoadProfile(string iProfileName)
    {
        // selected profile
    
        GameProgressSaveManager GPSM = Access.GameProgressSaveManager();
        GPSM.activeProfile = iProfileName;
        GPSM.Load();

        Access.CollectiblesManager().loadJars();

        if (GPSM.IsUniqueEventDone(UniqueEvents.UEVENTS.GP_IntroComplete))
            Access.SceneLoader().loadScene(Constants.SN_HUB);
        else
            Access.SceneLoader().loadScene(Constants.SN_INTRO);
    }

    public void OverWriteProfile(string iOldProfileName, string iNewProfileName)
    {
        // selected profile

        GameProgressSaveManager GPSM = Access.GameProgressSaveManager();
        
        // Erase fill if exists
        GPSM.DeleteProfileIfExists(iOldProfileName);

        // Create new profile
        GPSM.activeProfile = iNewProfileName;
        GPSM.CreateActiveProfile();
        GPSM.updateFilePath();
        GPSM.ResetAndSave();

        Access.SceneLoader().loadScene(Constants.SN_INTRO);
    }

}
