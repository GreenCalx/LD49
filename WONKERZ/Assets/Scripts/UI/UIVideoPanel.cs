using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIVideoPanel : UIPanelTabbed
{
    public TMP_Text currentResolution;
    public TMP_Text currentScreenMode;
    public UIPanelTabbed resolutionPanel;
    public UIPanelTabbed screenModePanel;
    public GameObject resolutionPanelElem;
    public GameObject screenModePanelElem;

    public RectTransform resScrollView;

    public void UpdateCurrentResolutionText(){
        currentResolution.text = Screen.currentResolution.width + " x " + Screen.currentResolution.height + " @" + Screen.currentResolution.refreshRate;
    }

    public void UpdateCurrentScreenModeText(){
        currentScreenMode.text = Screen.fullScreen ? "Fullscreen" : "Windowed";
    }

    public void InitResolutionPanel(){
        var res = Screen.resolutions;
        foreach(var r in res){
            var go = Instantiate(resolutionPanelElem, resolutionPanel.transform);

            go.GetComponent<TMP_Text>().text = r.width + " x " + r.height + " @" + r.refreshRate;
            var tab = go.GetComponent<UITab>();
            tab.Parent = resolutionPanel;
            tab.init();


            go.GetComponent<UIResolutionPanelElem>().idx = resolutionPanel.tabs.Count;
            go.GetComponent<UIResolutionPanelElem>().resScrollView = resScrollView;
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
        }
        else {
            //default
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = -1;
        }
    }

    public void OnTargetFPSChanged(string value){
        if (System.Int32.TryParse(value, out var number)){
            Application.targetFrameRate = number;
            Debug.Log(number);
        }
    }

    public override void activate(){
        this.gameObject.SetActive(true);
        base.activate();
        UpdateCurrentResolutionText();
        UpdateCurrentScreenModeText();
        InitResolutionPanel();
    }

    public override void deactivate(){
        base.deactivate();
        this.gameObject.SetActive(false);
    }
}
