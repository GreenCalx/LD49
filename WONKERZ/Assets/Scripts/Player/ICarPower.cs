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

public class BallCarPower : ICarPower
{
    public string name { get; set; }
    public GameObject modelObjectRef;

    private GameObject modelObjectInst;

    public BallCarPower(GameObject iModelObject)
    {
        name = "BallPower";
        modelObjectRef = iModelObject;
    }
    public void applyDirectEffect()
    {
        PlayerController player = Access.Player();

        modelObjectInst = GameObject.Instantiate(modelObjectRef);
        modelObjectInst.transform.position = player.transform.position;
        modelObjectInst.transform.rotation = player.transform.rotation;

        Rigidbody player_rb = player.car.rb;
        Rigidbody model_rb = modelObjectInst.GetComponent<Rigidbody>();
        if (model_rb && player_rb)
        {
            //model_rb.velocity = player_rb.velocity;
            model_rb.AddForce(player_rb.velocity, ForceMode.VelocityChange);
        }
    }
    public void onRefresh()
    {
        PlayerController cc = Access.Player();
        cc.transform.position = modelObjectInst.transform.position;
        cc.transform.rotation = modelObjectInst.transform.rotation;

        Rigidbody player_rb = cc.GetComponent<Rigidbody>();
        Rigidbody model_rb = modelObjectInst.GetComponent<Rigidbody>();
        if (model_rb && player_rb)
        {
            //model_rb.velocity = player_rb.velocity;
            player_rb.velocity = model_rb.velocity;
        }
    }
    public void onDisableEffect()
    {
        PlayerController cc = Access.Player();
        Rigidbody player_rb = cc.GetComponent<Rigidbody>();
        Rigidbody model_rb = modelObjectInst.GetComponent<Rigidbody>();
        if (model_rb && player_rb)
        {
            //model_rb.velocity = player_rb.velocity;
            player_rb.velocity = model_rb.velocity;
        }
        GameObject.Destroy(modelObjectInst);
    }
    public void applyEffectInInputs(InputManager.InputData iEntry, PlayerController iCC)
    {
        this.Log("Ball Input effects");
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
