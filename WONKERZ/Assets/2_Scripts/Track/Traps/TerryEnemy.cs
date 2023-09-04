using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerryEnemy : Trap
{
    public Animator animator;
    private readonly string chargeParm = "OnCharge";
    private readonly string releaseParm = "OnRelease";

    public GameObject shockwaveTorusRef;
    public Vector3 shockwavePosOffset;

    public AudioSource SFX_Stomp;
    public ParticleSystem PS_TerryDust;

    private GameObject shockwaveTorusInst;
    private bool shockwaveLaunched = false;

    // Start is called before the first frame update
    void Start()
    {
        shockwaveLaunched = false;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public override void OnTrigger(float iCooldownPercent)
    {
        animator.SetBool(releaseParm, true);

        if (!shockwaveLaunched)
        {
            if (iCooldownPercent >= 0.55f)
            {
                PS_TerryDust.Play();
            }
            if (iCooldownPercent >= 0.8f)
            {
                Schnibble.Utils.SpawnAudioSource( SFX_Stomp, transform);
                shockwaveTorusInst = Instantiate(shockwaveTorusRef);
                shockwaveTorusInst.transform.position = transform.position + shockwavePosOffset;
                TerryShockwave ts = shockwaveTorusInst.GetComponent<TerryShockwave>();
                if (!!ts)
                {
                    ts.launch();
                }
                shockwaveLaunched = true;
            }
        }

    }

    public override void OnCharge(float iCooldownPercent)
    {
        animator.SetBool(chargeParm, true);

    }

    public override void OnRest(float iCooldownPercent)
    {
        animator.SetBool(releaseParm, false);
        animator.SetBool(chargeParm, false);
        shockwaveLaunched = false;
        PS_TerryDust.Stop();

    }
}
