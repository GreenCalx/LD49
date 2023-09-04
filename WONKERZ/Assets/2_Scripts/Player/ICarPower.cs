using UnityEngine;
using Schnibble;

public interface ICarPower
{
    public string name { get; set; }

    public void applyDirectEffect();

    public void onRefresh();
    public void applyEffectInInputs(InputManager.InputData iEntry, PlayerController iCC);

    public bool turnOffTriggers();
    public void onDisableEffect();
}

public class NeutralCarPower : ICarPower
{
    public string name { get; set; }
    public NeutralCarPower()
    {
        name = "NeutralPower";
    }
    public void applyDirectEffect()
    {
        //Access.Player().SetMode(CarController.CarMode.NONE);
    }
    public void onRefresh()
    {
        // no power : nothing to do here 
    }
    public void onDisableEffect()
    {

    }
    public void applyEffectInInputs(InputManager.InputData iEntry, PlayerController iCC)
    {

    }
    public bool turnOffTriggers()
    {
        return false;
    }
}

public class TurboCarPower : ICarPower
{
    public GameObject turboParticlesRef;
    public GameObject turboParticlesInst;

    public string name { get; set; }

    public TurboCarPower(GameObject iTurboParticles)
    {
        name = "TurboPower";
        turboParticlesRef = iTurboParticles;
    }
    public void applyDirectEffect()
    {
        PlayerController player = Access.Player();

        turboParticlesInst = GameObject.Instantiate(turboParticlesRef);
        turboParticlesInst.transform.position = player.transform.position;
        turboParticlesInst.transform.rotation = player.transform.rotation;

        turboParticlesInst.GetComponent<ParticleSystem>()?.Play();
    }
    public void onRefresh()
    {
        PlayerController cc = Access.Player();
        cc.transform.position = turboParticlesInst.transform.position;
        cc.transform.rotation = turboParticlesInst.transform.rotation;
    }

    public void onDisableEffect()
    {
        GameObject.Destroy(turboParticlesInst);
    }

    public void applyEffectInInputs(InputManager.InputData iEntry, PlayerController iCC)
    {
        this.Log("Turbo Input effects");
        // No motor
    }
    public bool turnOffTriggers()
    {
        //if (Access.Player().currentSpeed < 1f)
        //    return true;
        return false;
    }
}

public class WaterCarPower : ICarPower
{
    public string name { get; set; }
    public WaterCarPower()
    {
        name = "WaterPower";
    }
    public void applyDirectEffect()
    {
        //Access.Player().SetMode(CarController.CarMode.WATER);
    }
    public void onRefresh()
    {

    }
    public void onDisableEffect()
    {

    }
    public void applyEffectInInputs(InputManager.InputData iEntry, PlayerController iCC)
    {
        this.Log("Water Input effects");
    }
    public bool turnOffTriggers()
    {
        //if (Access.Player().currentSpeed < 1f)
        //    return true;
        return false;
    }
}

public class PlaneCarPower : ICarPower
{
    public string name { get; set; }
    public PlaneCarPower()
    {
        name = "PlanePower";
    }
    public void applyDirectEffect()
    {
//        Access.Player().SetMode(CarController.CarMode.DELTA);
        //Access.Player().IsAircraft = true;
    }
    public void onRefresh()
    {

    }
    public void onDisableEffect()
    {
        //Access.Player().IsAircraft = false;
    }
    public void applyEffectInInputs(InputManager.InputData iEntry, PlayerController iCC)
    {
        this.Log("Plane Input effects");
    }
    public bool turnOffTriggers()
    {
        //if (Access.Player().currentSpeed < 1f)
        //    return true;
        return false;
    }
}

public class SpiderCarPower : ICarPower
{
    public string name { get; set; }
    public SpiderCarPower()
    {
        name = "SpiderPower";
    }
    public void applyDirectEffect()
    {
        //Access.Player().SetMode(CarController.CarMode.SPIDER);
    }
    public void onRefresh()
    {

    }
    public void onDisableEffect()
    {

    }
    public void applyEffectInInputs(InputManager.InputData iEntry, PlayerController iCC)
    {
        this.Log("Spider Input effects");
    }
    public bool turnOffTriggers()
    {
        //if (Access.Player().currentSpeed < 1f)
        //    return true;
        return false;
    }
}
