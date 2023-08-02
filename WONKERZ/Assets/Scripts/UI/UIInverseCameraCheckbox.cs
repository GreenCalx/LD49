using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInverseCameraCheckbox : UICheckbox
{
    public void GetBoolValue(UICheckboxValue v)
    {
        v.value = InputSettings.InverseRSMapping;
    }
    public void SetBoolValue(bool v)
    {
        InputSettings.InverseRSMapping = v;
    }
}
