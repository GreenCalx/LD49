using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UITextTab : UITab
{
    public TextMeshProUGUI text;

    public override void setColor(Color C)
    {
        text.color = C;
    }
}
