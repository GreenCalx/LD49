using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIImageTab : UITab
{
    public Image image;

    public override void setColor(Color C)
    {
        image.color = C;
    }
}

public class UIGarageCancelableImageTab : UIGarageCancelableTab
{
    public Image image;

    public override void setColor(Color C)
    {
        image.color = C;
    }
}
