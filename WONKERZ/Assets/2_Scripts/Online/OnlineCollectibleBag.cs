using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;

using Wonkerz; // Todo include me in namespace
using Mirror;

using Torque = Schnibble.SIUnits.KilogramMeter2PerSecond2;

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

        StartCoroutine(WaitPlayerForInit());
    }

    IEnumerator WaitPlayerForInit()
    {
        while (owner==null)
        {
            owner = Access.OfflineGameManager().localPlayer;
            yield return null;
        }

        if (isServer)
        {
            while (!NetworkRoomManagerExt.singleton.onlineGameManager.HasPlayerLoaded(owner))
            {
                yield return null;
            }
            RpcInitStatRefValues();
        }
            
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
        WkzCar wCar = owner.self_PlayerController.car.GetCar();

        accelInitValue = (float)wCar.motor.def.maxTorque;

        maxSpeedInitValue = (float)wCar.def.maxSpeedInKmH;

        // Expecting always at least one axle and all suspensions with same stiffness
        springInitStiffness = wCar.GetChassis().axles[0].right.GetSuspension().stiffness;
        springInitValue = wCar.wkzDef.jumpDef.stiffnessMul;

        turnInitValue = wCar.def.maxSteeringAngle;

        weightInitValue = owner.self_PlayerController.GetRigidbody().mass;
        //weightInitValue = 0f;

        torqueForceInitValue_X = wCar.wkzDef.weightControlMaxX;
        torqueForceInitValue_Z = wCar.wkzDef.weightControlMaxZ;
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
        WkzCar wCar = owner.self_PlayerController.car.GetCar();

        updateAccel(wCar);
        updateMaxSpeed(wCar);
        updateSprings(owner.self_PlayerController, wCar);
        updateTurn(wCar);
        updateTorqueForce(wCar);
        updateWeight();
    }

    [ClientRpc]
    public void RpcUpdateCar()
    {
        UpdateCar();
    }


    // TODO
    //   Modifiying SO feels weird
    // Ensure that its not saved, makes SetDirty() calls dangerous?
    // Probably needs a new layer that sets up /update car accordingly in OnlinePLayeR?

    private void updateAccel(WkzCar iWCar)
    {
        // remap between 0 and 1 with pivot at 0.5
        float curve_x = RemapStatToCurve(accels);
        iWCar.motor.def.maxTorque = (Torque)(accelCurve.Evaluate(curve_x) * accelInitValue);
    }
    
    private void updateMaxSpeed(WkzCar iWCar)
    {
        // remap between 0 and 1
        float curve_x = RemapStatToCurve(maxSpeeds);
        iWCar.def.maxSpeedInKmH = (maxSpeedCurve.Evaluate(curve_x) * maxSpeedInitValue);
    }

    private void updateSprings(PlayerController iPC, WkzCar iWCar)
    {
        // remap between 0 and 1
        float curve_x = RemapStatToCurve(springs);        

        // var new_stiffness = springCurve.Evaluate(curve_x) * springInitStiffness;
        // foreach(SchAxle ax in iWCar.GetChassis().axles)
        // {
        //     ax.right.GetSuspension().stiffness  = new_stiffness;
        //     ax.left.GetSuspension().stiffness   = new_stiffness;
        // }
        
        // iWCar.wkzDef.jumpDef.stiffnessMul = springCurve.Evaluate(curve_x) * springInitValue;
    }

    private void updateTurn(WkzCar iWCar)
    {
        float curve_x = RemapStatToCurve(turns);
    //    iWCar.def.maxSteeringAngle = turnCurve.Evaluate(curve_x) * turnInitValue;
    }

    private void updateTorqueForce(WkzCar iWCar)
    {
        // remap between 0 and 1
        float curve_x = RemapStatToCurve(torqueForces);

        // iWCar.wkzDef.weightControlMaxX = torqueForceCurve.Evaluate(curve_x) * torqueForceInitValue_X;
        // iWCar.wkzDef.weightControlMaxZ = torqueForceCurve.Evaluate(curve_x) * torqueForceInitValue_Z;
    }
    private void updateWeight()
    {
        // remap between 0 and 1
        float curve_x = RemapStatToCurve(weights);
        owner.self_PlayerController.GetRigidbody().mass = weightCurve.Evaluate(curve_x) * weightInitValue;
    }

    private float RemapStatToCurve(int iNumberOfStats)
    {
        float offset_from_pivot = (float)iNumberOfStats / (float)STATS_MAX_RANGE;
        return 0.5f + (offset_from_pivot * 0.5f);
    }

}
