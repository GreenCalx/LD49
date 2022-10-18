using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GasStation : MonoBehaviour
{
    public float nutConversionInterval = 0.2f;
    private float nutConversionElapsed = 99f;
    // Start is called before the first frame update
    void Start()
    {
        nutConversionElapsed = 99f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerStay(Collider iCollider)
    {
        if (Utils.isPlayer(iCollider.gameObject))
        {
            nutConversionElapsed += Time.deltaTime;
            if (nutConversionElapsed > nutConversionInterval)
            {
                Access.CollectiblesManager().tryConvertNutToTurbo();
                nutConversionElapsed = 0f;
            }
        }
    }
}
