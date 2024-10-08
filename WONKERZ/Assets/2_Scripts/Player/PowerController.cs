using System.Collections.Generic;
using UnityEngine;

using Schnibble;
using Schnibble.Managers;
namespace Wonkerz
{

    public class PowerController : MonoBehaviour
    {
        [Header("References for powers \n Legacy")]
        public GameObject turboParticles;
        public GameObject SpinPowerObject_Ref;
        [Header("KnightLance Launcher")]
        public GameObject KnightLanceObject_Ref;
        [Header("Pallet Launcher")]
        public GameObject PalletLauncherObject_Ref;
        public GameObject PalletRef;

        [Header("Powers")]
        public bool HasAPowerEquipped; 
        public List<ICarPower> powers;
        public ICarPower currentPower;
        public ICarPower nextPower;

        // Private cache

        void Awake()
        {
            powers = new List<ICarPower>()
            {
                null,
                new KnightLanceCarPower(KnightLanceObject_Ref),
                new PalletLauncherCarPower(PalletLauncherObject_Ref, PalletRef)
            };
        }

        // Start is called before the first frame update
        void Start()
        {
            currentPower = powers[0];
            HasAPowerEquipped = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (currentPower==null)
                return;

            if (currentPower.turnOffTriggers())
            {
                nextPower = powers[0];
                tryTriggerPower();
            }
            currentPower.onRefresh();
            refreshUI();
        }

        public bool isInNeutralPowerMode()
        {
            return (currentPower != powers[0]); // neutral power
        }

        public void tryTriggerPower()
        {
            if (nextPower != null)
            {
                //activate
                this.Log("Next power :" + nextPower.name);
                if (currentPower != null)
                    currentPower.onDisableEffect();

                currentPower = nextPower;
                currentPower.onEnableEffect();
            }
            nextPower = null; // reset next power
        }

        public void setNextPower(int iPowerIndex)
        {
            if ((iPowerIndex < 0) || (iPowerIndex >= powers.Count))
            { nextPower = null; return; }

            ICarPower carpower = powers[iPowerIndex];
            if (carpower == null)
            { nextPower = null; return; }

            nextPower = carpower;
        }

        public void EquipCollectiblePower(OnlineCollectible iCollectiblePower)
        {
            switch (iCollectiblePower.collectibleType)
            {
                case ONLINE_COLLECTIBLES.KLANCE_POWER:
                    setNextPower(1);

                    break;
                case ONLINE_COLLECTIBLES.PLAUNCHER:
                    setNextPower(2);

                    break;
                default:
                    break;
            }
            HasAPowerEquipped = true;
            tryTriggerPower();

            togglePowerUI();
        }

        public void UnequipPower()
        {
            if (currentPower!=null)
            {
                currentPower.onDisableEffect();
                currentPower = null;
                HasAPowerEquipped = false;
                togglePowerUI();
            }
        }

        public void applyPowerEffectInInputs(GameController iEntry, PlayerController iCC)
        {
            if (currentPower != null)
            {
                currentPower.applyEffectInInputs(iEntry, iCC);
            }
        }

        public void refreshUI()
        {
            if (currentPower==null)
                return;
            
            var ui = OnlineGameManager.singleton.UIPlayer;
            var ratio = Mathf.Clamp01(currentPower.elapsed_cooldown / currentPower.cooldown);
            ui.powerCooldownBar.fillAmount = ratio;
        }

        public void togglePowerUI()
        {
            var ui = OnlineGameManager.singleton.UIPlayer;
            if (currentPower == null)
            {
                ui.equippedPowerHandle.gameObject.SetActive(false);
                ui.DitchPowerHintHandle.gameObject.SetActive(false);
            }
                
            else {
                ui.equippedPowerHandle.gameObject.SetActive(true);
                ui.DitchPowerHintHandle.gameObject.SetActive(true);

                //ui.equippedPowerThumbnailImage = ...
            }
                
        }

    }
}
