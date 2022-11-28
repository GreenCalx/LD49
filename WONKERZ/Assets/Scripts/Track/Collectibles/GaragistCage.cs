using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GaragistCage : MonoBehaviour
{
    public string openingKeyName;

    public Animator cageAnim;
    private string openCageAnimParm = "unlock";

    void OnTriggerEnter(Collider iCollider)
    {
        if (Utils.isPlayer(iCollider.gameObject))
        {
            if (Access.CollectiblesManager().hasCageKey(openingKeyName))
            {
                // open cage
                cageAnim.SetBool( openCageAnimParm, true);
            }
        }
    }
}
