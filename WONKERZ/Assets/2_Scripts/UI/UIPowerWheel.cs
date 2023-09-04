using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPowerWheel : MonoBehaviour
{
    public Image wheelImage;

    public PowerController.PowerWheelPlacement neutral = PowerController.PowerWheelPlacement.NEUTRAL;
    public Image neutral_Image;

    public PowerController.PowerWheelPlacement power1;
    public Image power1_Image;

    public PowerController.PowerWheelPlacement power2;
    public Image power2_Image;

    public PowerController.PowerWheelPlacement power3;
    public Image power3_Image;

    public PowerController.PowerWheelPlacement power4;
    public Image power4_Image;

    public Dictionary<PowerController.PowerWheelPlacement, Image> selectors;

    // Start is called before the first frame update
    void Start()
    {
        selectors =
        new Dictionary<PowerController.PowerWheelPlacement, Image>()
        {
            {neutral, neutral_Image},
            {power1, power1_Image},
            {power2, power2_Image},
            {power3, power3_Image},
            {power4, power4_Image},
        };
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void showWheel(bool iToggle)
    {
        wheelImage.enabled = iToggle;
    }

    public void hideAll()
    {
        foreach (PowerController.PowerWheelPlacement pwp in selectors.Keys)
        {
            selectors[pwp].enabled = false;
        }
    }

    public void showSelector(PowerController.PowerWheelPlacement iToShow)
    {
        foreach (PowerController.PowerWheelPlacement pwp in selectors.Keys)
        {
            if (iToShow == pwp)
            { selectors[pwp].enabled = true; continue; }
            selectors[pwp].enabled = false;
        }
    }
}
