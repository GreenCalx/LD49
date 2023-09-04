using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUBGaragist : MonoBehaviour
{
    public string name = "";
    public string trackname = "";
    // Start is called before the first frame update
    void Start()
    {
        CollectiblesManager cm = Access.CollectiblesManager();
        if (cm.hasGaragist(trackname))
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
        }
    }
}
