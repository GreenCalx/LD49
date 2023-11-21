using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Schnibble.UI;

public class UIInverseCameraCheckbox : UICheckbox
{
    public void GetBoolValue(UICheckboxValue v)
    {
        v.value = InputSettings.InverseCameraMappingX;
    }
    public void SetBoolValue(bool v)
    {
        InputSettings.InverseCameraMappingX = v;
    }
}
