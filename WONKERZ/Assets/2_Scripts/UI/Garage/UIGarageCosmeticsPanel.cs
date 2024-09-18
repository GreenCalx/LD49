using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Schnibble;
using Schnibble.UI;

namespace Wonkerz {
    /**
     * MAIN, DOORS, HOOD, BUMPS, WHEEL
     */

    public class UIGarageCosmeticsPanel : UIGaragePanel
    {
        [Header("--------------------------------\n# UIGarageCosmeticsPanel")]
        public GameObject uiGaragePickableColorRef;
        public GameObject uiGaragePickableSkinRef;
        public GameObject uiGaragePickableDecalRef;

        [Header("-BodyPanel-")]
        public UIGaragePanel uiNavParentForCar;

        public UIPanelTabbed primaryCarColorHandle;
        public UIPanelTabbed frameSkinHandle;
        public UIPanelTabbed hoodSkinHandle;
        public UIPanelTabbed doorSkinHandle;

        public UIPanelTabbed secondaryCarColorHandle;
        public UIPanelTabbed lampsSkinHandle;
        public UIPanelTabbed frontPipesSkinHandle;
        public UIPanelTabbed backPipesSkinHandle;
        public UIPanelTabbed bumpersSkinHandle;
        public UIPanelTabbed windshieldSkinHandle;

        public List<COLORIZABLE_CAR_PARTS> primaryCarColorParts;
        public List<COLORIZABLE_CAR_PARTS> secondaryCarColorParts;

        [Header("-WheelPanel-")]
        public UIGaragePanel uiNavParentForWheels;
        public UIPanelTabbed primaryWheelColorHandle;
        public UIPanelTabbed wheelSkinHandle;
        public List<COLORIZABLE_CAR_PARTS> primaryWheelColorParts;

        [Header("-AccessoriesPanel-")]
        public UIGaragePanel uiNavParentForAccessories;
        public UIPanelTabbed wheightSkinHandle;
        public UIPanelTabbed jumpDecalHandle;
        public UIPanelTabbed springsSkinHandle;

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

        override protected void Start()
        {
            refreshAvailableCosmetics();
        }

        public void refreshAvailableCosmetics()
        {
            // init from table
            PlayerCosmeticsManager pcm = Access.PlayerCosmeticsManager();
        
            // populate default cosmetics
            List<CosmeticElement> defaults = pcm.getDefaultCarParts();
            foreach(CosmeticElement c in defaults) 
            { 
                dispatchCosmeticElementInUI(c);
            }

            // populate unlocked cosmetics
            foreach (int skin_id in pcm.availableCosmetics)
            {
                CosmeticElement c_el = pcm.getCosmeticFromID(skin_id);
                if (c_el==null)
                continue;
                c_el.skinID = skin_id;
                dispatchCosmeticElementInUI(c_el);
            }
        
        }

        public void dispatchCosmeticElementInUI(CosmeticElement iCE)
        {
            switch (iCE.cosmeticType)
            {
                case CosmeticType.PAINT:
                    addBodyColor(iCE);
                    break;
                case CosmeticType.MODEL:
                    addSkin(iCE);
                    break;
                case CosmeticType.DECAL:
                    addDecal(iCE);
                    break;
                case CosmeticType.RUBBER:
                    addWheelColor(iCE);
                    break;
                default:
                    break;
            }
        }

        /// ---

        private void addBodyColor(CosmeticElement iCE)
        {
            addColor(primaryCarColorHandle, uiNavParentForCar, primaryCarColorParts, iCE);
            addColor(secondaryCarColorHandle, uiNavParentForCar, secondaryCarColorParts, iCE);
        }

        private void addWheelColor(CosmeticElement iCE)
        {
            addColor(primaryWheelColorHandle, uiNavParentForWheels, primaryWheelColorParts, iCE);
        }

        private void addColor(UIPanelTabbed iParent, UIGaragePanel iNavParent, List<COLORIZABLE_CAR_PARTS> iParts, CosmeticElement iCosmetic)
        {
            GameObject go = Instantiate(uiGaragePickableColorRef, iParent.transform);
            UIGaragePickableColor uigpc = go.GetComponent<UIGaragePickableColor>();
            Image ui_img = go.GetComponent<Image>();

            uigpc.Parent = iNavParent;
            uigpc.useParentInputs = true;
            uigpc.parts_to_colorize = iParts;
            uigpc.material = iCosmetic.material;
            //Material mat = Resources.Load(iCosmetic.matName, typeof(Material)) as Material;

            ui_img.color = iCosmetic.material.color; 

            iParent.tabs.Add(uigpc);
        }

        private void addSkin(CosmeticElement iCosmetic)
        {
            UIPanelTabbed parent = null;
            UIGaragePanel navparent = null;

            // find part
            switch (iCosmetic.carPart)
            {
                case COLORIZABLE_CAR_PARTS.MAIN:
                    parent = frameSkinHandle;
                    navparent = uiNavParentForCar;
                    break;
                case COLORIZABLE_CAR_PARTS.HOOD:
                    parent = hoodSkinHandle;
                    navparent = uiNavParentForCar;
                    break;
                case COLORIZABLE_CAR_PARTS.LEFT_DOOR:
                    parent = doorSkinHandle;
                    navparent = uiNavParentForCar;
                    break;
                case COLORIZABLE_CAR_PARTS.RIGHT_DOOR:
                    parent = doorSkinHandle;
                    navparent = uiNavParentForCar;
                    break;
                case COLORIZABLE_CAR_PARTS.FRONT_BUMP:
                    parent = bumpersSkinHandle;
                    navparent = uiNavParentForCar;
                    break;
                case COLORIZABLE_CAR_PARTS.BACK_BUMP:
                    parent = bumpersSkinHandle;
                    navparent = uiNavParentForCar;
                    break;
                case COLORIZABLE_CAR_PARTS.LAMPS:
                    parent = lampsSkinHandle;
                    navparent = uiNavParentForCar;
                    break;
                case COLORIZABLE_CAR_PARTS.FRONT_PIPES:
                    parent = frontPipesSkinHandle;
                    navparent = uiNavParentForCar;
                    break;
                case COLORIZABLE_CAR_PARTS.BACK_PIPES:
                    parent = backPipesSkinHandle;
                    navparent = uiNavParentForCar;
                    break;
                case COLORIZABLE_CAR_PARTS.WINDSHIELD:
                    parent = windshieldSkinHandle;
                    navparent = uiNavParentForCar;
                    break;
                case COLORIZABLE_CAR_PARTS.WHEELS:
                    parent = wheelSkinHandle;
                    navparent = uiNavParentForWheels;
                    break;
                case COLORIZABLE_CAR_PARTS.WEIGHT:
                    parent = wheightSkinHandle;
                    navparent = uiNavParentForAccessories;
                    break;
                case COLORIZABLE_CAR_PARTS.SPRINGS:
                    parent = springsSkinHandle;
                    navparent = uiNavParentForWheels;
                    break;
            }

            GameObject go = Instantiate(uiGaragePickableSkinRef, parent.transform);
            UIGaragePickableSkin uigps = go.GetComponent<UIGaragePickableSkin>();
            Image ui_img = go.GetComponent<Image>();

            uigps.Parent = navparent;
            uigps.useParentInputs = true;
            uigps.carPart = iCosmetic.carPart;
            uigps.skinName = iCosmetic.name;
        
            //ui_img.color = mat.color; 

            parent.tabs.Add(uigps);
        
            if (iCosmetic.isDefaultSkin)
            uigps.setSkinID(-1);
            else
            uigps.setSkinID(iCosmetic.skinID);
        }

        private void addDecal(CosmeticElement iCosmetic)
        {
            UIPanelTabbed parent = null;
            UIGaragePanel navparent = null;

            // find part
            switch (iCosmetic.carPart)
            {
                case COLORIZABLE_CAR_PARTS.JUMP_DECAL:
                    parent = jumpDecalHandle;
                    navparent = uiNavParentForAccessories;
                    break;
            }


            GameObject go = Instantiate(uiGaragePickableDecalRef, parent.transform);
            UIGaragePickableDecal uigpd = go.GetComponent<UIGaragePickableDecal>();
            Image ui_img = go.GetComponent<Image>();

            uigpd.Parent = navparent;
            uigpd.useParentInputs= true;
            uigpd.carPart = iCosmetic.carPart;
            uigpd.skinName = iCosmetic.name;

            parent.tabs.Add(uigpd);

            if (iCosmetic.isDefaultSkin)
            uigpd.setSkinID(-1);
            else
            uigpd.setSkinID(iCosmetic.skinID);
        
            //ui_img.color = mat.color; 

        
        }

    }
}
