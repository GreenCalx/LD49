using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using static UnityEngine.Debug;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

using Schnibble;
using Schnibble.UI;



namespace Wonkerz {

    public class UIProfileCards : UIPanelTabbed
    {
        public enum FD_MODE {READ, WRITE};
        public FD_MODE mode;

        public UIInputField createProfilePanel;

        override public void Init()
        {
            base.Init();

            List<string> profiles = Access.managers.gameProgressSaveMgr.GetAvailableProfileNames();
            for(int i=0; i < tabs.Count; ++i)
            {
                if (i >= profiles.Count || string.IsNullOrEmpty(profiles[i])) {
                    (tabs[i] as UITextTab).label.content = "Empty";
                } else {
                    (tabs[i] as UITextTab).label.content = profiles[i];
                }
            }
        }

        public void OnEndEditNewName(string oldName) {
            (tabs[selected] as UITextTab).label.content = createProfilePanel.text.content;
            // TODO: validate inputs.
            string newProfileName = createProfilePanel.text.content;

            OverWriteProfile(oldName, newProfileName);
        }

        public void activateSelectedCard()
        {
            string selectedProfileName = (tabs[selected] as UITextTab).label.content;
            if (string.IsNullOrEmpty(selectedProfileName) || selectedProfileName.ToLower() == "empty" || mode == FD_MODE.WRITE) {
                this.Log("Empty profile selected : launch creation panel.");
                createProfilePanel.onInputModified = new UnityEvent();
                createProfilePanel.onInputModified.AddListener(() => OnEndEditNewName(selectedProfileName));
                createProfilePanel.placeholderText = "Enter your name here...";
                createProfilePanel.Show();
                createProfilePanel.Activate();
                return;
            }

            LoadProfile(selectedProfileName);
        }

        public void LoadProfile(string iProfileName)
        {
            // selected profile

            GameProgressSaveManager GPSM = Access.managers.gameProgressSaveMgr;
            GPSM.SwitchActiveProfile(iProfileName);

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
            GPSM.OverwriteProfile   (iOldProfileName, iNewProfileName);
            GPSM.SwitchActiveProfile(iNewProfileName);

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
