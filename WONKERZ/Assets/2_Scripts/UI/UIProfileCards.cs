using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Debug;
using Schnibble;
using Schnibble.UI;
using System;
using UnityEngine.SceneManagement;


namespace Wonkerz {

    public class UIProfileCards : UIPanelTabbed
    {
        public enum FD_MODE {READ, WRITE};
        public FD_MODE mode;
        public List<UIProfileCardPanel> profileCards;
        public string newProfileName = "NewPlayer";
        public UIPanel createProfilePanel;

        public void updateNewProfileName(string iS)
        {
            newProfileName = iS;
        }

        override public void init()
        {
            base.init();

            profileCards = new List<UIProfileCardPanel>(tabs.Count);
            foreach(UITab t in tabs)
            {
                UIProfileCardPanel uipcp = (UIProfileCardPanel) t;
                if (uipcp==null)
                {
                    this.LogError("A card profile doesn't carry UIProfileCardPanel : " + t.gameObject.name);
                    continue;
                }
                profileCards.Add(uipcp);
            }

            List<string> profiles = Access.managers.gameProgressSaveMgr.GetAvailableProfileNames();
            for (int i=0; i < profiles.Count; i++)
            {
                if (i >= profileCards.Count)
                break;
                profileCards[i].profileName.text = profiles[i];
            }
        }

        public void OnEndEditNewName() {
            UIProfileCardPanel uipcp = (UIProfileCardPanel) tabs[selected];
            if (uipcp==null)
            {
                this.LogError("Selected Card is not activable");
                return;
            }
            string selected_profile_name = uipcp.profileName.text;

            OverWriteProfile(selected_profile_name, newProfileName);
            uipcp.profileName.text = newProfileName;
        }

        public void activateSelectedCard()
        {
            UIProfileCardPanel uipcp = (UIProfileCardPanel) tabs[selected];
            if (uipcp==null)
            {
                this.LogError("Selected Card is not activable");
                return;
            }
            string selected_profile_name = uipcp.profileName.text;
            if (string.IsNullOrEmpty(selected_profile_name) || selected_profile_name.ToLower() == "empty") {
                this.Log("Empty profile selected : launch creation panel.");
                createProfilePanel.Show();
                return;
            }

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

            GameProgressSaveManager GPSM = Access.managers.gameProgressSaveMgr;
            GPSM.activeProfile = iProfileName;
            GPSM.Load();

            // :NoOnline:
            // NOTE: toffa: this is not used for online mode anymore.
            // objects should listen to load profile event from GameProgressSaveManager.
            #if false
            DialogBank.playerName = iProfileName;

            Access.managers.collectiblesMgr.loadJars();

            if (GPSM.IsUniqueEventDone(UniqueEvents.UEVENTS.GP_IntroComplete))
            Access.managers.sceneMgr.loadScene(Constants.SN_HUB, new SceneLoader.LoadParams
            {
                useTransitionIn = true,
                useTransitionOut = true,
                useLoadingScene = true,
                sceneLoadingMode = LoadSceneMode.Single,
            });
            else
            Access.managers.sceneMgr.loadScene(Constants.SN_INTRO, new SceneLoader.LoadParams
            {
                useTransitionIn = true,
                useTransitionOut = true,
                useLoadingScene = true,
            });
            #endif
        }

        public void OverWriteProfile(string iOldProfileName, string iNewProfileName)
        {
            // selected profile

            GameProgressSaveManager GPSM = Access.managers.gameProgressSaveMgr;

            // Erase fill if exists
            GPSM.DeleteProfileIfExists(iOldProfileName);

            // Create new profile
            GPSM.activeProfile = iNewProfileName;
            GPSM.CreateActiveProfile();
            GPSM.updateFilePath();
            GPSM.ResetAndSave();

            // cf :NoOnline:
            #if false
            DialogBank.playerName = iNewProfileName;

            Access.managers.sceneMgr.loadScene(Constants.SN_INTRO, new SceneLoader.LoadParams
            {
                useTransitionIn = true,
                useTransitionOut = true,
                useLoadingScene = true,
            });
            #endif
        }

    }
}
