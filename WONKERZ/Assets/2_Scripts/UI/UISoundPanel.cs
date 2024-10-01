using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Schnibble.UI;

public class UISoundPanel : UIPanelTabbed
{
    public UISlider volumeSlider;

    override public void Activate() {
        base.Activate();

        volumeSlider.cursorHintFormat = "P1"; // show as percent
        volumeSlider.value = GetMainVolume();
        volumeSlider.ValueChanged();

        volumeSlider.onValueChange.AddListener( () => OnSliderMainVolumeChanged(volumeSlider.valueNormalized) );
    }

    override public void Deactivate() {
        base.Deactivate();

        volumeSlider.onValueChange.RemoveAllListeners();
    }

    public void OnSliderMainVolumeChanged(float value){
        AudioListener.volume = value;
    }

    public float GetMainVolume() {
        return AudioListener.volume;
    }
}
