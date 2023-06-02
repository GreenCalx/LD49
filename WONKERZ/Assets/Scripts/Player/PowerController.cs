using System.Collections.Generic;
using UnityEngine;

using Schnibble;

public class PowerController : MonoBehaviour
{
    public enum PowerWheelPlacement { NEUTRAL, UP, DOWN, LEFT, RIGHT }

    public GameObject ballPowerObject;

    public List<ICarPower> powers;
    public Dictionary<ICarPower, bool> unlockedPowers = new Dictionary<ICarPower, bool>();
    public ICarPower currentPower;
    public ICarPower nextPower;

    // Private cache
    private UIPowerWheel uiPowerWheel;

    void Awake()
    {
        powers = new List<ICarPower>()
        {
            new NeutralCarPower(),
            new BallCarPower(ballPowerObject),
            new WaterCarPower(),
            new PlaneCarPower(),
            new SpiderCarPower()
        };
    }

    // Start is called before the first frame update
    void Start()
    {
        showUI(false);
        // Load Unlocked powers
        // unlock everything in the meantime
        foreach (ICarPower cp in powers)
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

        if (!!unlockedPowers[carpower])
        {
            nextPower = carpower;
        }
        else
        {
            this.Log("Power is locked : " + carpower.name);
        }
    }

    public void applyPowerEffectInInputs(InputManager.InputData iEntry, PlayerController iCC)
    {
        if (currentPower != null)
        {
            currentPower.applyEffectInInputs(iEntry, iCC);
        }
    }

    public void showUI(bool iToggle)
    {
        if (uiPowerWheel == null)
        {
            uiPowerWheel = Access.UIPowerWheel();
        }
        if (!!uiPowerWheel)
        {
            uiPowerWheel.showWheel(iToggle);
        }
        else
        {
            this.LogWarn("Unable to find UIPowerWheel.");
        }
    }

    public void showIndicator(PowerWheelPlacement iPlacement)
    {
        if (uiPowerWheel == null)
        {
            uiPowerWheel = Access.UIPowerWheel();
        }
        if (!!uiPowerWheel)
        {
            uiPowerWheel.showSelector(iPlacement);
        }
        else
        {
            this.LogWarn("Unable to find UIPowerWheel.");
        }
    }

    public void hideIndicators()
    {
        if (uiPowerWheel == null)
        { uiPowerWheel = Access.UIPowerWheel(); }

        if (!!uiPowerWheel)
        {
            uiPowerWheel.hideAll();
        }
        else
        {
            this.LogWarn("Unable to find UIPowerWheel.");
        }
    }
}
