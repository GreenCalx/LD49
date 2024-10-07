using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using Schnibble;
using Schnibble.UI;

public class UIVideoPanel : UIPanelTabbed
{
    public UILabel currentResolution;
    public UILabel currentScreenMode;

    // TODO: move this dircetly inside the resulution panel
    public UIPanelTabbed resolutionPanel;
    public UITextTab resolutionPanelElem;

    public UISlider targetFPSSlider;
    public UITab    targetFPSTab;

    public void UpdateCurrentResolutionText(){
        currentResolution.content = Screen.currentResolution.width + " x " + Screen.currentResolution.height + " @" + Screen.currentResolution.refreshRate;
    }

    public void UpdateCurrentScreenModeText(){
        currentScreenMode.content = Screen.fullScreen ? "Fullscreen" : "Windowed";
    }

    public void InitResolutionPanel(){
        var res = Screen.resolutions;
        foreach(var r in res){
            var tab = Instantiate(resolutionPanelElem, resolutionPanel.transform);

            tab.parent = resolutionPanel;
            tab.label.content = r.width + " x " + r.height + " @" + r.refreshRate;
            tab.index = resolutionPanel.tabs.Count;

            tab.Init();

            resolutionPanel.tabs.Add(tab);
        }
    }

    public void SetFullScreen(){
        var res = Screen.currentResolution;
        Screen.SetResolution( res.width, res.height, FullScreenMode.ExclusiveFullScreen);
        UpdateCurrentScreenModeText();
    }

    public void SetWindowed(){
        var res = Screen.currentResolution;
        Screen.SetResolution( res.width, res.height, FullScreenMode.Windowed);
        UpdateCurrentScreenModeText();
    }

    public void SetBorderless(){
        var res = Screen.currentResolution;
        Screen.SetResolution( res.width, res.height, FullScreenMode.FullScreenWindow);
        UpdateCurrentScreenModeText();
    }

    public void IsVSyncOn(UICheckbox.UICheckboxValue v){
        v.value = QualitySettings.vSyncCount != 0;
    }

    public void OnVSyncValueChanged(bool value){
        if(value) {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = Screen.currentResolution.refreshRate;

            // remove target fps tab
            tabs.Remove(targetFPSTab);
            targetFPSTab.Hide();
        }
        else {
            //default
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;

            if (!tabs.Contains(targetFPSTab)) {
                tabs.Add(targetFPSTab);
            }

            targetFPSTab.Show();
        }

        PlayerPrefs.SetInt("vsync", QualitySettings.vSyncCount);
    }

    public override void Init() {
        base.Init();

        targetFPSSlider.Show();
    }

    public override void Activate(){
        base.Activate();

        targetFPSSlider.value = PlayerPrefs.GetInt("targetFPS");
        targetFPSSlider.onValueChange.AddListener(() => PlayerPrefs.SetInt("targetFPS", (int)targetFPSSlider.value));
        targetFPSSlider.ValueChanged();

        UpdateCurrentResolutionText();
        UpdateCurrentScreenModeText();
        InitResolutionPanel();

        OnVSyncValueChanged(QualitySettings.vSyncCount != 0);
    }

    public override void Deactivate() {
        base.Deactivate();

        targetFPSSlider.onValueChange.RemoveAllListeners();
        targetFPSSlider.Hide();
    }
}
