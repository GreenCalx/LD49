using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Schnibble.UI;

public class UISoundPanel : UIPanelControlable
{
    public void OnSliderMainVolumeChanged(float value){
        AudioListener.volume = value;
    }
}
