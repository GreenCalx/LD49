using System.Collections.Generic;
using UnityEngine;

public class TrapTrigger : MonoBehaviour
{

    public List<Trap> toTrigger;

    void OnTriggerEnter(Collider iCollider)
    {
        if (Utils.isPlayer(iCollider.gameObject))
        {
            foreach (Trap t in toTrigger)
            {
                t.OnTrigger();
            }
        }
    }
}
