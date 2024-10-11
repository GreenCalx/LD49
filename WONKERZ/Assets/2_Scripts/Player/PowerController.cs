using System.Collections.Generic;
using UnityEngine;

using Schnibble;
using Schnibble.Managers;
namespace Wonkerz
{

    public class PowerController : MonoBehaviour
    {
        [Header("Powers")]
        public bool HasAPowerEquipped;
        public PowerCollection powerCollection;
        public List<ICarPower> powers;
        public ICarPower currentPower;
        public ICarPower nextPower;

        [Header("Tweaks")]
        public GameObject prefabDitchPowerPS;

        void Start()
        {
            // currentPower = powers[0];
            currentPower = null;
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

        public void EquipCollectiblePower(OnlineCollectible iCollectiblePower)
        {
            PlayerPowerElement ppe = powerCollection.GetPowerFromCollectible(iCollectiblePower.collectibleType);

            ppe.carPower.init(ppe);
            nextPower = ppe.carPower;

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

                if (prefabDitchPowerPS!=null)
                {
                    GameObject ps_inst = Instantiate(prefabDitchPowerPS);
                    
                    WeightIndicator WI = GetComponentInChildren<WeightIndicator>();

                    ps_inst.transform.parent = WI.transform;
                    ps_inst.transform.localPosition = Vector3.zero;
                    ParticleSystem as_ps = ps_inst.GetComponent<ParticleSystem>();
                    Destroy(ps_inst, as_ps.main.duration);
                }
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
            ui.updateCarPowerUI(currentPower);
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

                ui.equippedPowerThumbnailImage.sprite = currentPower.fullPowerDef.powerImage;
            }
                
        }

    }
}
