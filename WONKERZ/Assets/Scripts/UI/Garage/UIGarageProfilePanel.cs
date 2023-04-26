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
        profile.color_bumps     = PlayerColorManager.Instance.getColorOfPart(COLORIZABLE_CAR_PARTS.FRONT_BUMP);
        profile.color_doors     = PlayerColorManager.Instance.getColorOfPart(COLORIZABLE_CAR_PARTS.LEFT_DOOR);
        profile.color_hood      = PlayerColorManager.Instance.getColorOfPart(COLORIZABLE_CAR_PARTS.HOOD);
        profile.color_wheels    = PlayerColorManager.Instance.getColorOfPart(COLORIZABLE_CAR_PARTS.WHEELS);

        profile.skin_body = PlayerSkinManager.Instance.getCustomizationOfPart(COLORIZABLE_CAR_PARTS.MAIN);
        profile.skin_back_bump = PlayerSkinManager.Instance.getCustomizationOfPart(COLORIZABLE_CAR_PARTS.BACK_BUMP);
        profile.skin_front_bump = PlayerSkinManager.Instance.getCustomizationOfPart(COLORIZABLE_CAR_PARTS.FRONT_BUMP);
        profile.skin_hood = PlayerSkinManager.Instance.getCustomizationOfPart(COLORIZABLE_CAR_PARTS.HOOD);
        profile.skin_left_door = PlayerSkinManager.Instance.getCustomizationOfPart(COLORIZABLE_CAR_PARTS.LEFT_DOOR);
        profile.skin_right_door = PlayerSkinManager.Instance.getCustomizationOfPart(COLORIZABLE_CAR_PARTS.RIGHT_DOOR);
        profile.skin_wheel = PlayerSkinManager.Instance.getCustomizationOfPart(COLORIZABLE_CAR_PARTS.WHEELS);

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
        PlayerColorManager.Instance.colorize(profile.color_bumps, COLORIZABLE_CAR_PARTS.FRONT_BUMP);
        PlayerColorManager.Instance.colorize(profile.color_bumps, COLORIZABLE_CAR_PARTS.BACK_BUMP);
        PlayerColorManager.Instance.colorize(profile.color_doors, COLORIZABLE_CAR_PARTS.LEFT_DOOR);
        PlayerColorManager.Instance.colorize(profile.color_doors, COLORIZABLE_CAR_PARTS.RIGHT_DOOR);
        PlayerColorManager.Instance.colorize(profile.color_hood, COLORIZABLE_CAR_PARTS.HOOD);
        PlayerColorManager.Instance.colorize(profile.color_wheels, COLORIZABLE_CAR_PARTS.WHEELS);

        PlayerSkinManager.Instance.customize(profile.skin_body);
        PlayerSkinManager.Instance.customize(profile.skin_hood);
        PlayerSkinManager.Instance.customize(profile.skin_back_bump);
        PlayerSkinManager.Instance.customize(profile.skin_front_bump);
        PlayerSkinManager.Instance.customize(profile.skin_right_door);
        PlayerSkinManager.Instance.customize(profile.skin_left_door);
        PlayerSkinManager.Instance.customize(profile.skin_wheel);
    }
}
