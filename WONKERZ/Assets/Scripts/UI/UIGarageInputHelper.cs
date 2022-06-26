using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UIGarageInputHelper : MonoBehaviour
{
    private List<UIGarageHelperElement> availableHelperBoxes;
    private int n_helperBoxes;

    void Awake()
    {
        availableHelperBoxes = new List<UIGarageHelperElement>(GetComponentsInChildren<UIGarageHelperElement>());
        n_helperBoxes = availableHelperBoxes.Count;
    }
    public void refreshHelper(IUIGarageElement iUIInterface)
    {
        Dictionary<string,string> newHelperText = iUIInterface.getHelperInputs();
        int i_helper = 0;
        
        n_helperBoxes = availableHelperBoxes.Count;
        if (n_helperBoxes <= 0 )
        { Debug.LogWarning("No Helper Boxes available."); return; }

        foreach ( string input in newHelperText.Keys )
        {
            UIGarageHelperElement el = availableHelperBoxes[i_helper];
            Sprite img = Resources.Load<Sprite>(input);
            el.img_elem.sprite = img;

            string txt = "";
            newHelperText.TryGetValue(input, out txt);
            el.txt_elem.text = txt;

            i_helper++;
            if (i_helper >= n_helperBoxes)
                break; // no more helpers available
        }
        // erase unused helper boxes
        if ( i_helper < n_helperBoxes )
        {
            Sprite blank_img = Resources.Load<Sprite>(Constants.RES_ICON_BLANK);
            for (int i=i_helper;i<n_helperBoxes;i++)
            {
                UIGarageHelperElement el = availableHelperBoxes[i];
                el.img_elem.sprite = blank_img;
                el.txt_elem.text = "";
            }
        }
    }
}
