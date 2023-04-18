using System.Collections.Generic;
using UnityEngine;
using Schnibble;

public class UIGarageProfilePanel : UIGaragePanel
{
    //public int n_slots = 5;    
    [Header("MAND")]
    [SerializeField]
    private UIGarageProfile profile;
    public UIGarageActionConfirmPanel confirmPanel;
    private int i_profile;

    override protected List<IUIGarageElement.UIGarageHelperValue> getHelperInputs()
    {
        return new List<IUIGarageElement.UIGarageHelperValue>{
            new IUIGarageElement.UIGarageHelperValue(Constants.RES_ICON_A, "SAVE"),
            new IUIGarageElement.UIGarageHelperValue(Constants.RES_ICON_Y, "LOAD"),
            new IUIGarageElement.UIGarageHelperValue(Constants.RES_ICON_B, "CANCEL")
        };
    }

    public void save(string profile_name)
    {
        SaveAndLoad.datas.Add(profile);

        fillProfileFromPlayerCC();

        if (!!confirmPanel)
        {
            confirmPanel.Parent = this;
            confirmPanel.setTextBoxField("SAVE ?");
            confirmPanel.setConfirmAction(() => SaveAndLoad.save(profile_name));
            confirmPanel.onActivate?.Invoke();
        }

    }

    public void fillProfileFromPlayerCC()
    {
        CarController cc = (Parent as UIGarage).getGarageEntry().playerCC;
        // TORQUE
        //profile.TORQUE_CURVE = new List<Keyframe>(cc.torqueCurve.keys);
        // WEIGHT
        //profile.WEIGHT_CURVE = new List<Keyframe>(cc.s.keys);
        
        // COLOR
        profile.color           = PlayerColorManager.Instance.getColorOfPart(COLORIZABLE_CAR_PARTS.MAIN);
        profile.color_bumps     = PlayerColorManager.Instance.getColorOfPart(COLORIZABLE_CAR_PARTS.BUMPS);
        profile.color_doors     = PlayerColorManager.Instance.getColorOfPart(COLORIZABLE_CAR_PARTS.DOORS);
        profile.color_hood      = PlayerColorManager.Instance.getColorOfPart(COLORIZABLE_CAR_PARTS.HOOD);
        profile.color_wheels    = PlayerColorManager.Instance.getColorOfPart(COLORIZABLE_CAR_PARTS.WHEELS);
    }

    public void load(string profile_name)
    {
        SaveAndLoad.datas.Add(profile);
        if (!!confirmPanel)
        {
            confirmPanel.Parent = this;
            confirmPanel.setTextBoxField("LOAD ?");
            confirmPanel.setConfirmAction(() => updatePlayerFromProfile(profile_name));
            confirmPanel.onActivate?.Invoke();
        }
    }

    public void updatePlayerFromProfile(string profile_name)
    {
        SaveAndLoad.loadGarageProfile(profile_name, profile);

        CarController cc = (Parent as UIGarage).getGarageEntry().playerCC;
        // TORQUE
        //cc.torqueCurve = new AnimationCurve(profile.TORQUE_CURVE.ToArray());
        // WEIGHT
        //cc.WEIGHT = new AnimationCurve(profile.WEIGHT_CURVE.ToArray());
        //color
        PlayerColorManager.Instance.colorize(profile.color, COLORIZABLE_CAR_PARTS.MAIN);
        PlayerColorManager.Instance.colorize(profile.color_bumps, COLORIZABLE_CAR_PARTS.BUMPS);
        PlayerColorManager.Instance.colorize(profile.color_doors, COLORIZABLE_CAR_PARTS.DOORS);
        PlayerColorManager.Instance.colorize(profile.color_hood, COLORIZABLE_CAR_PARTS.HOOD);
        PlayerColorManager.Instance.colorize(profile.color_wheels, COLORIZABLE_CAR_PARTS.WHEELS);
    }
}
