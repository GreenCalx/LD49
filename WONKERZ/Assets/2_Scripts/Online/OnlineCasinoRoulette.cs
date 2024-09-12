using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;
using Wonkerz;

public class OnlineCasinoRoulette : NetworkBehaviour
{
    public Rigidbody self_RB;
    public float spinForce = 50000f;
    public float angularVelocityThreshold = 50f;
    public float currentAngularVelocityMag = 0f;
    [Header("Ball")]
    public GameObject rouletteBallPrefab;
    private GameObject rouletteBallInst;
    public Transform BallSpawnPointHandle;


    [SyncVar]
    public bool isSpinning = false;

    void Start()
    {
        if (self_RB==null)
            self_RB = GetComponent<Rigidbody>();

        self_RB.maxAngularVelocity = angularVelocityThreshold + 1f;

        rouletteBallInst = Instantiate(rouletteBallPrefab);
        NetworkServer.Spawn(rouletteBallInst);
        rouletteBallInst.transform.position = BallSpawnPointHandle.position;
    }

    void Update()
    {
        if (isSpinning)
        {
            currentAngularVelocityMag =    self_RB.angularVelocity.magnitude;
            if (self_RB.angularVelocity.magnitude <= Vector3.one.magnitude)
            {
                isSpinning = false;
                self_RB.angularVelocity = Vector3.zero;
            }
        }
    }

    [Server]
    public void Spin()
    {
        rouletteBallInst.transform.position = BallSpawnPointHandle.position;
        
        self_RB.AddTorque( transform.up * spinForce, ForceMode.Impulse);     
        isSpinning = true;
    }

    /// Based on the fact that only local player can fire actions from CameraFocusable
    /// Thus we (hopefully) can't reach these lines  if we are not local player
    public void PlayRoulette()
    {
        if (isClientOnly)
        {
            CmdPlayRoulette();
            return;
        }

        self_RB.angularVelocity = Vector3.zero;
        Spin();
        currentAngularVelocityMag =    self_RB.angularVelocity.magnitude;
    }

    [Command]
    public void CmdPlayRoulette()
    {
        PlayRoulette();
    }

}
