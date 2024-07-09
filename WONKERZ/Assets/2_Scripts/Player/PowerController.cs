using System.Collections.Generic;
using UnityEngine;

using Schnibble;
using Schnibble.Managers;
namespace Wonkerz
{

    public class PowerController : MonoBehaviour
    {
        public enum PowerWheelPlacement { NEUTRAL, UP, DOWN, LEFT, RIGHT }

        [Header("References for powers")]
        public GameObject turboParticles;
        public GameObject SpinPowerObject_Ref;
        public GameObject KnightLanceObject_Ref;
        [Header("Powers")]
        public List<ICarPower> powers;
        //public Dictionary<ICarPower, bool> unlockedPowers = new Dictionary<ICarPower, bool>();
        public ICarPower currentPower;
        public ICarPower nextPower;

        // Private cache
        private UIPowerWheel uiPowerWheel;

        void Awake()
        {
            powers = new List<ICarPower>()
            {
                new NeutralCarPower(),
                new SpinCarPower(SpinPowerObject_Ref),
                new KnightLanceCarPower(KnightLanceObject_Ref)
            };
        }

        // Start is called before the first frame update
        void Start()
        {

            // Load Unlocked powers
            // unlock everything in the meantime
            // CollectiblesManager CM = Access.CollectiblesManager();
            // foreach (ICarPower cp in powers)
            // {
            //     unlockedPowers.Add(cp, true);
            // }
            currentPower = powers[0];
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
            refreshPower();
        }

        public bool isInNeutralPowerMode()
        {
            return (currentPower != powers[0]); // neutral power
        }

        public void refreshPower()
        {
            if (currentPower != null)
            currentPower.onRefresh();
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
                currentPower.applyDirectEffect();
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

            // if (!!unlockedPowers[carpower])
            // {
                nextPower = carpower;
            // }
            // else
            // {
            //     this.Log("Power is locked : " + carpower.name);
            // }
        }

        public void EquipCollectiblePower(OnlineCollectible iCollectiblePower)
        {
            switch (iCollectiblePower.collectibleType)
            {
                case ONLINE_COLLECTIBLES.KLANCE_POWER:
                    setNextPower(2);
                    break;
                default:
                    break;
            }
            tryTriggerPower();

            refreshUI();
            //currentPower = iPowerToEquip;
            //setNextPower(iPowerIDToEquip);
        }

        public void UnequipPower()
        {
            if (currentPower!=null)
            {
                currentPower.onDisableEffect();
                currentPower = null;

                refreshUI();
            }
        }

        public void applyPowerEffectInInputs(GameInput[] iEntry, PlayerController iCC)
        {
            if (currentPower != null)
            {
                currentPower.applyEffectInInputs(iEntry, iCC);
            }
        }

        public void refreshUI()
        {
            if (currentPower == null)
                Access.UIPlayerOnline().equippedPower.text = "--";
            else
                Access.UIPlayerOnline().equippedPower.text = currentPower.name;
        }

    }
}
