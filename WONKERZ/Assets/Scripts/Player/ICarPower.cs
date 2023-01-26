using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICarPower
{
    public string name {get;set;}

    public void applyDirectEffect();
    public void onDisableEffect();
    public void applyEffectInInputs(InputManager.InputData iEntry, CarController iCC);
    public bool turnOffTriggers();
}

public class NeutralCarPower : ICarPower
{
    public string name {get;set;}
    public NeutralCarPower()
    {
        name = "NeutralPower";
    }
    public void applyDirectEffect()
    {
        Access.Player().SetMode(CarController.CarMode.NONE);
    }
    public void onDisableEffect()
    {

    }
    public void applyEffectInInputs(InputManager.InputData iEntry, CarController iCC)
    {

    }
    public bool turnOffTriggers()
    {
        return false;
    }
}

public class BallCarPower : ICarPower
{
    public string name {get;set;}
    public BallCarPower()
    {
        name = "BallPower";
    }
    public void applyDirectEffect()
    {
        Access.Player().SetMode(CarController.CarMode.BALL);
    }
    public void onDisableEffect()
    {

    }
    public void applyEffectInInputs(InputManager.InputData iEntry, CarController iCC)
    {
        //Debug.Log("Ball Input effects");
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
    public string name {get;set;}
    public WaterCarPower()
    {
        name = "WaterPower";
    }
    public void applyDirectEffect()
    {
        Access.Player().SetMode(CarController.CarMode.WATER);
    }
    public void onDisableEffect()
    {

    }
    public void applyEffectInInputs(InputManager.InputData iEntry, CarController iCC)
    {
        //Debug.Log("Water Input effects");
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
    public string name {get;set;}
    public PlaneCarPower()
    {
        name = "PlanePower";
    }
    public void applyDirectEffect()
    {
        Access.Player().SetMode(CarController.CarMode.DELTA);
        Access.Player().IsAircraft = true;
    }
    public void onDisableEffect()
    {
        Access.Player().IsAircraft = false;
    }
    public void applyEffectInInputs(InputManager.InputData iEntry, CarController iCC)
    {
        //Debug.Log("Plane Input effects");
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
    public string name {get;set;}
    public SpiderCarPower()
    {
        name = "SpiderPower";
    }
    public void applyDirectEffect()
    {
        Access.Player().SetMode(CarController.CarMode.SPIDER);
    }
    public void onDisableEffect()
    {

    }
    public void applyEffectInInputs(InputManager.InputData iEntry, CarController iCC)
    {
        //Debug.Log("Spider Input effects");
    }
    public bool turnOffTriggers()
    {
        //if (Access.Player().currentSpeed < 1f)
        //    return true;
        return false;
    }
}