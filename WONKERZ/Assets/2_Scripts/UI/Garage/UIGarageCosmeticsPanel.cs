using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Schnibble;

/**
* MAIN, DOORS, HOOD, BUMPS, WHEEL
*/

public class UIGarageCosmeticsPanel : UIGaragePanel
{
    [Header("UIGarageCosmeticsPanel")]
    public UIPanelTabbed primaryCarColorHandle;
    public UIPanelTabbed secondaryCarColorHandle;
    public UIPanelTabbed primaryWheelColorHandle;
    
    public UIPanelTabbed carSkinHandle;
    public UIPanelTabbed wheelSkinHandle;

    public UIGaragePanel uiNavParentForCar;
    public UIGaragePanel uiNavParentForWheels;

    public List<COLORIZABLE_CAR_PARTS> primaryCarColorParts;
    public List<COLORIZABLE_CAR_PARTS> secondaryCarColorParts;
    public List<COLORIZABLE_CAR_PARTS> primaryWheelColorParts;

    public List<COLORIZABLE_CAR_PARTS> carSkinParts;
    public List<COLORIZABLE_CAR_PARTS> wheelSkinParts;

    public GameObject uiGaragePickableColorRef;
    public GameObject uiGaragePickableSkinRef;


    override protected List<IUIGarageElement.UIGarageHelperValue> getHelperInputs()
    {
        return new List<IUIGarageElement.UIGarageHelperValue>{
            new IUIGarageElement.UIGarageHelperValue(Constants.RES_ICON_A, "OK"),
            new IUIGarageElement.UIGarageHelperValue(Constants.RES_ICON_B, "CANCEL")
        };
    }

    public void selectCategory()
    {
        // Part to modify
    }

    void Start()
    {
        refreshAvailableCosmetics();
    }

    public void refreshAvailableCosmetics()
    {
        // init from table
        PlayerCosmeticsManager pcm = Access.PlayerCosmeticsManager();
        
        foreach (CosmeticElement c_el in pcm.availableCosmetics)
        {
            // Init available colors
            if (c_el.matName != "")
            {
                if (primaryCarColorParts.Contains(c_el.carPart))
                {
                    addColor(primaryCarColorHandle, uiNavParentForCar, primaryCarColorParts, c_el);
                }
                else if (secondaryCarColorParts.Contains(c_el.carPart))
                {
                    addColor(secondaryCarColorHandle, uiNavParentForCar, secondaryCarColorParts, c_el);
                }
                else if (primaryWheelColorParts.Contains(c_el.carPart))
                {
                    addColor(primaryWheelColorHandle, uiNavParentForWheels, primaryWheelColorParts, c_el);
                }
            }

            // Init available skins
            if (c_el.modelName != "")
            {
                if ( carSkinParts.Contains(c_el.carPart) )
                {
                    addSkin( carSkinHandle, uiNavParentForCar, carSkinParts, c_el);
                }
                else if ( wheelSkinParts.Contains(c_el.carPart))
                {
                    addSkin( wheelSkinHandle, uiNavParentForWheels, wheelSkinParts, c_el);
                }
            }
        }
        
    }

    private void addColor(UIPanelTabbed iParent, UIGaragePanel iNavParent, List<COLORIZABLE_CAR_PARTS> iParts, CosmeticElement iCosmetic)
    {
        GameObject go = Instantiate(uiGaragePickableColorRef, iParent.transform);
        UIGaragePickableColor uigpc = go.GetComponent<UIGaragePickableColor>();
        Image ui_img = go.GetComponent<Image>();

        uigpc.Parent = iNavParent;
        uigpc.copyInputsFromParent = true;
        uigpc.parts_to_colorize = iParts;
        uigpc.material_name = iCosmetic.matName;
        Material mat = Resources.Load(iCosmetic.matName, typeof(Material)) as Material;

        ui_img.color = mat.color; 

        iParent.tabs.Add(uigpc);
    }

    private void addSkin(UIPanelTabbed iParent, UIGaragePanel iNavParent, List<COLORIZABLE_CAR_PARTS> iParts, CosmeticElement iCosmetic)
    {
        GameObject go = Instantiate(uiGaragePickableSkinRef, iParent.transform);
        UIGaragePickableSkin uigps = go.GetComponent<UIGaragePickableSkin>();
        Image ui_img = go.GetComponent<Image>();

        uigps.Parent = iNavParent;
        uigps.copyInputsFromParent = true;
        uigps.carParts = iParts;
        uigps.skinName = iCosmetic.modelName;

        //ui_img.color = mat.color; 

        iParent.tabs.Add(uigps);
    }


}
