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
        PlayerController player = (Parent as UIGarage).getGarageEntry().player;
        // TORQUE
        //profile.TORQUE_CURVE = new List<Keyframe>(cc.torqueCurve.keys);
        // WEIGHT
        //profile.WEIGHT_CURVE = new List<Keyframe>(cc.s.keys);
        
        // COSMETICS
        profile.skin_body       = Access.PlayerCosmeticsManager().getCustomizationOfPart(COLORIZABLE_CAR_PARTS.MAIN);
        profile.skin_back_bump  = Access.PlayerCosmeticsManager().getCustomizationOfPart(COLORIZABLE_CAR_PARTS.BACK_BUMP);
        profile.skin_front_bump = Access.PlayerCosmeticsManager().getCustomizationOfPart(COLORIZABLE_CAR_PARTS.FRONT_BUMP);
        profile.skin_hood       = Access.PlayerCosmeticsManager().getCustomizationOfPart(COLORIZABLE_CAR_PARTS.HOOD);
        profile.skin_left_door  = Access.PlayerCosmeticsManager().getCustomizationOfPart(COLORIZABLE_CAR_PARTS.LEFT_DOOR);
        profile.skin_right_door = Access.PlayerCosmeticsManager().getCustomizationOfPart(COLORIZABLE_CAR_PARTS.RIGHT_DOOR);
        profile.skin_wheel      = Access.PlayerCosmeticsManager().getCustomizationOfPart(COLORIZABLE_CAR_PARTS.WHEELS);

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

        PlayerController cc = (Parent as UIGarage).getGarageEntry().player;
        // TORQUE
        //cc.torqueCurve = new AnimationCurve(profile.TORQUE_CURVE.ToArray());
        // WEIGHT
        //cc.WEIGHT = new AnimationCurve(profile.WEIGHT_CURVE.ToArray());
        //color
        PlayerCosmeticsManager PCM = Access.PlayerCosmeticsManager();

        PCM.colorize(profile.skin_body.materialName, COLORIZABLE_CAR_PARTS.MAIN);
        PCM.colorize(profile.skin_hood.materialName, COLORIZABLE_CAR_PARTS.HOOD);
        PCM.colorize(profile.skin_back_bump.materialName, COLORIZABLE_CAR_PARTS.BACK_BUMP);
        PCM.colorize(profile.skin_front_bump.materialName, COLORIZABLE_CAR_PARTS.FRONT_BUMP);
        PCM.colorize(profile.skin_right_door.materialName, COLORIZABLE_CAR_PARTS.RIGHT_DOOR);
        PCM.colorize(profile.skin_left_door.materialName, COLORIZABLE_CAR_PARTS.LEFT_DOOR);
        PCM.colorize(profile.skin_wheel.materialName, COLORIZABLE_CAR_PARTS.WHEELS);

        PCM.customize(profile.skin_body);
        PCM.customize(profile.skin_hood);
        PCM.customize(profile.skin_back_bump);
        PCM.customize(profile.skin_front_bump);
        PCM.customize(profile.skin_right_door);
        PCM.customize(profile.skin_left_door);
        PCM.customize(profile.skin_wheel);
    }
}
