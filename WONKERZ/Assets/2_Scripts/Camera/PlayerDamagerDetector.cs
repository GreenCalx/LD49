using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Child of a player camera
// Display dangers behind the player
// ATM, designed to have 3 of em for leftCorner/Middle/RightCorner with their respective uis
// alpha of UIToDisplay defined by the closest distance within damagers in range
public class PlayerDamagerDetector : MonoBehaviour
{
    public GameObject UIToDisplay_Ref;
    private GameObject UIToDisplay_Inst;
    public bool dangerInRange = false;
    private List<PlayerDamager> damagersInRange;

    // Start is called before the first frame update
    void Start()
    {
        damagersInRange = new List<PlayerDamager>();
    }

    void Update()
    {
        if (dangerInRange)
        {
            refreshUI();
        } else if (!!UIToDisplay_Inst) {
            hideUI();
        }
    }

    void hideUI()
    {
        Destroy(UIToDisplay_Inst);
        UIToDisplay_Inst = null;
    }

    void refreshUI()
    {
        if (UIToDisplay_Inst==null)
        { 
            Transform parent = Access.UISpeedAndLifePool().transform.parent;
            UIToDisplay_Inst = Instantiate<GameObject>(UIToDisplay_Ref, parent); 
        }

        Image img = UIToDisplay_Inst.GetComponentInChildren<Image>();
        if (img==null)
        {
            Debug.LogError("Image missing on ui to display given to playerdamagerdetector.");
            return;
        }

        float closestDistToPlayer = 9999f;
        Vector3 playerPos = Access.Player().transform.position;
        foreach(PlayerDamager pd in damagersInRange)
        {
            float dist = Vector3.Distance(playerPos, pd.transform.position);
            if (dist < closestDistToPlayer)
            {
                closestDistToPlayer = dist;
            }
        }

        // Alpha ramp with closts object..
        //img.color.a = 0.5f;
        
    }

    void OnTriggerStay(Collider iCollider)
    {
        PlayerDamager pd = iCollider.gameObject.GetComponent<PlayerDamager>();
        if (!!pd)
        {
            if (!damagersInRange.Contains(pd))
                damagersInRange.Add(pd);
            updateDangerInRange();
        }
    }

    void OnTriggerExit(Collider iCollider)
    {
        PlayerDamager pd = iCollider.gameObject.GetComponent<PlayerDamager>();
        if (!!pd)
        {
            if (damagersInRange.Contains(pd))
                damagersInRange.Remove(pd);
            updateDangerInRange();
        }
    }

    void updateDangerInRange()
    {
        dangerInRange = (damagersInRange.Count > 0 );
    }
}
