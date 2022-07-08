using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGarageTestEnd : MonoBehaviour
{
    private UIGarageTestManager uigtm;
    // Start is called before the first frame update
    void Start()
    {
        uigtm = Utils.getTestManager();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider iCollider)
    {
        if (iCollider.GetComponent<CarController>())
        {
            // TEST SUCCESSFULL
            uigtm.endTest(true);
        }
    }
}
