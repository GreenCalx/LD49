using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerController : MonoBehaviour
{
    public enum PowerWheelPlacement { NEUTRAL, UP, DOWN, LEFT, RIGHT}

    public List<ICarPower> powers = new List<ICarPower>()
    {
        new NeutralCarPower(),
        new BallCarPower(),
        new WaterCarPower(),
        new PlaneCarPower(),
        new SpiderCarPower()
    };
    public Dictionary<ICarPower,bool> unlockedPowers = new Dictionary<ICarPower,bool>();
    public ICarPower currentPower;
    public ICarPower nextPower;

    // Private cache
    private UIPowerWheel uiPowerWheel;
    // Start is called before the first frame update
    void Start()
    {
        showUI(false);
        // Load Unlocked powers
        // unlock everything in the meantime
        foreach ( ICarPower cp in powers )
        {
            unlockedPowers.Add(cp, true);
        }
        currentPower = powers[0];
    }

    // Update is called once per frame
    void Update()
    {
        if (currentPower.turnOffTriggers())
        {
            nextPower = powers[0];
            tryTriggerPower();
        }
    }

    public void tryTriggerPower()
    {
        if (nextPower!=null)
        {
            //activate
            Debug.Log("Next power :" + nextPower.name);
            currentPower.onDisableEffect();
            currentPower = nextPower;
            currentPower.applyDirectEffect();
        }
        nextPower = null; // reset next power
    }

    public void setNextPower(int iPowerIndex)
    {
        if ((iPowerIndex < 0)||(iPowerIndex>=powers.Count))
        { nextPower = null; return;}

        ICarPower carpower = powers[iPowerIndex];
        if (carpower==null)
        { nextPower = null; return;}

        if (!!unlockedPowers[carpower])
        { 
            nextPower = carpower;
        } else {
            Debug.Log("Power is locked : " + carpower.name);
        }
    }

    public void applyPowerEffectInInputs(InputManager.InputData iEntry, CarController iCC)
    {
        if (currentPower != null)
        {
            currentPower.applyEffectInInputs(iEntry,iCC);
        }
    }

    public void showUI(bool iToggle)
    {
        if (uiPowerWheel==null)
        {
            uiPowerWheel = Access.UIPowerWheel();
        }
        if (!!uiPowerWheel)
        {
            uiPowerWheel.showWheel(iToggle);
        } else {
            Debug.LogWarning("Unable to find UIPowerWheel.");
        }
    }

    public void showIndicator(PowerWheelPlacement iPlacement)
    {
        if (uiPowerWheel==null)
        {
            uiPowerWheel = Access.UIPowerWheel();
        }
        if (!!uiPowerWheel)
        {
            uiPowerWheel.showSelector(iPlacement);
        } else {
            Debug.LogWarning("Unable to find UIPowerWheel.");
        }
    }

    public void hideIndicators()
    {
        if (uiPowerWheel==null)
        { uiPowerWheel = Access.UIPowerWheel(); }

        if (!!uiPowerWheel)
        {
            uiPowerWheel.hideAll();
        } else {
            Debug.LogWarning("Unable to find UIPowerWheel.");
        }
    }
}
