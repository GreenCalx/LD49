using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialTrick : MonoBehaviour
{
    public string name;
    public float trickValue = 100f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerStay(Collider iCollider)
    {
        if (Utils.colliderIsPlayer(iCollider))
        {
            
        }

    }
}
