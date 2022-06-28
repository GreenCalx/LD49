using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGarageTestManager : MonoBehaviour
{
    public GameObject startTest;
    public GameObject endTest;
    private GameObject testCC;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void launchTest()
    {
        if (testCC!=null)
            Destroy(testCC.gameObject);

        // clone player to start test position
        testCC = Instantiate(Utils.getPlayerRef(), transform);
        UIGarageTestStart uigts = startTest.GetComponent<UIGarageTestStart>();
        
        CarController cc = testCC.GetComponent<CarController>();
        cc.isFrozen = false;
        testCC.transform.position = uigts.respawnPoint.position;
        //Utils.detachControllable<CarController>(cc);
        Utils.attachUniqueControllable(cc);
        
        updateLayers( testCC, Utils.getLayerIndex(Constants.LYR_UIMESH));

        // switch input manager to automod
        Utils.GetInputManager().CurrentMode = InputManager.Mode.AUTOPILOT;

        // launch simulation until it reaches end position
        
    }

    public void updateLayers(GameObject iGO, int iLayer)
    {
        if (iGO==null)
            return;

        iGO.layer = iLayer;
        foreach( Transform child in iGO.transform )
        {
            if (null==child)
                continue;
            updateLayers(child.gameObject, iLayer);
        }
    }

    public void quitTest()
    {
        if (testCC!=null)
        {
            Utils.detachUniqueControllable(testCC.GetComponent<CarController>());
            Destroy(testCC.gameObject);
        }
    }
}
