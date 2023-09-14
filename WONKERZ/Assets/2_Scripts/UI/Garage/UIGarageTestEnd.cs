using UnityEngine;
using Schnibble;

public class UIGarageTestEnd : MonoBehaviour
{
    #if false
    private UIGarageTestManager uigtm;
    // Start is called before the first frame update
    void Start()
    {
        uigtm = Access.TestManager();
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
    #endif
}
