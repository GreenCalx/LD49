using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;
using Mirror;

public class OnlineCollectibleBag : NetworkBehaviour
{
    public OnlinePlayerController owner;

    /// STATS DEFINITION
    [Header("Stats definition curves, {X=0.5, Y=1} is Initial Value for 0 Stats")]
    // Needs to be symetrical to have a pivot on 0.5 for the 0 value
    public const int STATS_MAX_RANGE = 10;
    public const int STATS_MIN_RANGE = -1 * STATS_MAX_RANGE;
    [Header("Init values (autoset at start from owner)")]
    [SyncVar]
    public float accelInitValue;
    [SyncVar]
    public float maxSpeedInitValue;
    [SyncVar]
    public float springInitValue;
    [SyncVar]
    public float springInitStiffness;
    [SyncVar]
    public float turnInitValue;
    [SyncVar]
    public float torqueForceInitValue_X;
    [SyncVar]
    public float torqueForceInitValue_Z;
    [SyncVar]
    public float weightInitValue;

    [Header("Stats definition curves\n    {X=0} is MIN stat range \n    {X=0.5, Y=1} is Initial Value for 0 Stats \n    {X=1} is MAX stat range")]
    //[ToolTip("X : From Stats Min Range [X=0] to Stats Max Range [X=1] \n Y : From 0 [Y=0] to 2x Base value [Y=1]")]
    
    public AnimationCurve accelCurve;
    public AnimationCurve maxSpeedCurve;
    public AnimationCurve springCurve;
    public AnimationCurve turnCurve;
    public AnimationCurve torqueForceCurve;
    public AnimationCurve weightCurve;

    [Header("Internal storage")]
    [SyncVar]
    public int nuts;
    [SyncVar]
    public int accels;
    [SyncVar]
    public int maxSpeeds;
    [SyncVar]
    public int springs;
    [SyncVar]
    public int turns;
    [SyncVar]
    public int torqueForces;
    [SyncVar]
    public int weights;


    // Start is called before the first frame update
    void Start()
    {
        owner = Access.OfflineGameManager().localPlayer;
        nuts = 0;
        accels = 0;
        maxSpeeds = 0;
        springs = 0;
        turns = 0;
        torqueForces = 0;
        weights = 0;

        if (isServer)
            RpcInitStatRefValues();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ClientRpc]
    public void RpcInitStatRefValues()
    {
        InitStatRefValues();
    }

    public void InitStatRefValues()
    {
        CarController cc = owner.self_PlayerController.car;

        maxSpeedInitValue = cc.maxTorque;
        springInitStiffness = cc.springStiffness;
        springInitValue = owner.self_PlayerController.jump.value;
        turnInitValue = cc.maxSteeringAngle_deg;
        weightInitValue = cc.rb.mass;
        torqueForceInitValue_X = owner.self_PlayerController.weightControlMaxX;
        torqueForceInitValue_Z = owner.self_PlayerController.weightControlMaxZ;
    }

    [Server]
    public void AsServerCollect(OnlineCollectible iCollectible)
    {
        switch (iCollectible.collectibleType)
        {
            case ONLINE_COLLECTIBLES.NUTS:
                nuts += iCollectible.value;
                return;
            case ONLINE_COLLECTIBLES.ACCEL:
                accels += iCollectible.value;
                accels = Mathf.Clamp(accels, STATS_MIN_RANGE, STATS_MAX_RANGE);
                break;
            case ONLINE_COLLECTIBLES.MAX_SPEED:
                maxSpeeds += iCollectible.value;
                maxSpeeds = Mathf.Clamp(maxSpeeds, STATS_MIN_RANGE, STATS_MAX_RANGE);
                break;
            case ONLINE_COLLECTIBLES.SPRINGS:
                springs += iCollectible.value;
                springs = Mathf.Clamp(springs, STATS_MIN_RANGE, STATS_MAX_RANGE);
                break;
            case ONLINE_COLLECTIBLES.TURN:
                turns += iCollectible.value;
                turns = Mathf.Clamp(turns, STATS_MIN_RANGE, STATS_MAX_RANGE);
                break;
            case ONLINE_COLLECTIBLES.TORQUEFORCE:
                torqueForces += iCollectible.value;
                torqueForces = Mathf.Clamp(torqueForces, STATS_MIN_RANGE, STATS_MAX_RANGE);
                break;
            case ONLINE_COLLECTIBLES.WEIGHT:
                weights += iCollectible.value;
                weights = Mathf.Clamp(weights, STATS_MIN_RANGE, STATS_MAX_RANGE);
                break;
            default:
                break;
        }
        if (isServerOnly)
            UpdateCar();
        else
            RpcUpdateCar();
    }

    [Command]
    public void CmdCollect(OnlineCollectible iCollectible)
    {
        AsServerCollect(iCollectible);
    }


    /// STATS
    public void UpdateCar()
    {
        CarController cc = owner.self_PlayerController.car;

        updateAccel(cc);
        updateMaxSpeed(cc);
        updateSprings(owner.self_PlayerController, cc);
        updateTurn(cc);
        updateTorqueForce(owner.self_PlayerController);
        updateWeight(cc);
    }

    [ClientRpc]
    public void RpcUpdateCar()
    {
        UpdateCar();
    }

    private void updateAccel(CarController iCC)
    {
        // remap between 0 and 1 with pivot at 0.5
        float curve_x = RemapStatToCurve(accels);
        //iCC.maxTorque = accelCurve.Evaluate(curve_x) * accelInitValue;
    }
    
    private void updateMaxSpeed(CarController iCC)
    {
        // remap between 0 and 1
        float curve_x = RemapStatToCurve(maxSpeeds);
        iCC.maxTorque = maxSpeedCurve.Evaluate(curve_x) * maxSpeedInitValue;
    }

    private void updateSprings(PlayerController iPC, CarController iCC)
    {
        // remap between 0 and 1
        float curve_x = RemapStatToCurve(springs);
        //iCC.maxTorque = accelCurve.Evaluate(curve_x) * accelInitValue;
        iCC.springStiffness = springCurve.Evaluate(curve_x) * springInitStiffness;
        iPC.jump.value = springCurve.Evaluate(curve_x) * springInitValue;
    }

    private void updateTurn(CarController iCC)
    {
        float curve_x = RemapStatToCurve(turns);
        iCC.maxSteeringAngle_deg = turnCurve.Evaluate(curve_x) * turnInitValue;
    }

    private void updateTorqueForce(PlayerController iPC)
    {
        // remap between 0 and 1
        float curve_x = RemapStatToCurve(torqueForces);

        iPC.weightControlMaxX = torqueForceCurve.Evaluate(curve_x) * torqueForceInitValue_X;
        iPC.weightControlMaxZ = torqueForceCurve.Evaluate(curve_x) * torqueForceInitValue_Z;
    }
    private void updateWeight(CarController iCC)
    {
        // remap between 0 and 1
        float curve_x = RemapStatToCurve(weights);
        iCC.rb.mass = weightCurve.Evaluate(curve_x) * weightInitValue;
    }

    private float RemapStatToCurve(int iNumberOfStats)
    {
        float offset_from_pivot = (float)iNumberOfStats / (float)STATS_MAX_RANGE;
        return 0.5f + (offset_from_pivot * 0.5f);
    }

}