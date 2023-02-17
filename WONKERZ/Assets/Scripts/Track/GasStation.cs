using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasStation : MonoBehaviour
{
    public Animator animator;

    public float nutConversionInterval = 0.2f;
    private float nutConversionElapsed = 99f;

    private bool IsPumpingGas = false;
    private string animatorParm = "IsPumping";

    public bool bypassNutsCost = false;

    // Start is called before the first frame update
    void Start()
    {
        nutConversionElapsed = 99f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider iCollider)
    {
        if (Utils.isPlayer(iCollider.gameObject))
        {
            Access.CheckPointManager().notifyGasStation(this);
        }
    }

    void OnTriggerStay(Collider iCollider)
    {
        if (Utils.isPlayer(iCollider.gameObject))
        {
            if (bypassNutsCost)
            {
                Access.CollectiblesManager().currentTurbo = 1f;
                Access.UITurboAndLifePool().updateTurboBar(Access.CollectiblesManager().currentTurbo);
                return;
            }
            nutConversionElapsed += Time.deltaTime;
            if (nutConversionElapsed > nutConversionInterval)
            {
                bool convertSuccess = Access.CollectiblesManager().tryConvertNutToTurbo();
                nutConversionElapsed = 0f;
                if ( convertSuccess && !IsPumpingGas)
                {
                    animator.SetBool(animatorParm, true);
                    IsPumpingGas = true;
                } else if ( !convertSuccess && IsPumpingGas ) {
                    animator.SetBool(animatorParm, false);
                    IsPumpingGas = false;
                }
            }
        }
    }

    void OnTriggerExit(Collider iCollider)
    {
        if (Utils.isPlayer(iCollider.gameObject))
        {
            if (IsPumpingGas)
            {
                IsPumpingGas = false;
                animator.SetBool(animatorParm, false);
            }
                
        }
    }
}
