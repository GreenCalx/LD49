using System.Collections.Generic;
using UnityEngine;
public class UIGarageInputHelper : MonoBehaviour
{
    private List<UIGarageHelperElement> availableHelperBoxes;
    private int n_helperBoxes;

    public GameObject helperBox_ref;

    void Awake()
    {
        availableHelperBoxes = new List<UIGarageHelperElement>();
    }
    public void refreshHelper(IUIGarageElement iUIInterface)
    {
        // this should probably NOT be a dict at all... this is an array with value (string,string)
        List<IUIGarageElement.UIGarageHelperValue> newHelperText = iUIInterface.getHelperInputs();

        int helperIdx = 0;
        while (helperIdx < availableHelperBoxes.Count && helperIdx < newHelperText.Count)
        {
            // if existing helper, update it
            var el = availableHelperBoxes[helperIdx];
            var val = newHelperText[helperIdx];

            el.img_elem.sprite = Resources.Load<Sprite>(val.imgName);
            el.txt_elem.text = val.txt;

            helperIdx++;
        }

        while (helperIdx < newHelperText.Count)
        {
            // create new helpers
            var el = GameObject.Instantiate(helperBox_ref, gameObject.transform);
            el.SetActive(true);
            availableHelperBoxes.Add(el.GetComponent<UIGarageHelperElement>());

            var val = newHelperText[helperIdx];

            var el_itf = el.GetComponent<UIGarageHelperElement>();
            el_itf.img_elem.sprite = Resources.Load<Sprite>(val.imgName);
            el_itf.txt_elem.text = val.txt;

            helperIdx++;
        }

        var range = (availableHelperBoxes.Count - 1) - helperIdx;
        if (range > 0)
        {
            while (helperIdx < availableHelperBoxes.Count)
            {
                // remove uneeded helpers
                GameObject.Destroy(availableHelperBoxes[helperIdx].gameObject);
            }
            availableHelperBoxes.RemoveRange(availableHelperBoxes.Count - range, range);
        }
    }
}
