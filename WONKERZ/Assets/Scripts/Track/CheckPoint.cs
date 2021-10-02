using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    private bool hasManager = false;
    public Transform respawn_location;
    private CheckPointManager cpm;
    // Start is called before the first frame update
    void Start()
    {
        hasManager = subscribeToManager();

    }


    // Update is called once per frame
    void Update()
    {
        if (!hasManager)
            hasManager = subscribeToManager();
    }

    public bool subscribeToManager()
    {
        GameObject manager = GameObject.Find(Constants.GO_CPManager);
        cpm = manager.GetComponent<CheckPointManager>();
        if (!!cpm)
        {
            cpm.subscribe(this.gameObject);
            return true;
        }
        return false;
    }

    void OnTriggerEnter(Collider iCol)
    {
        CarController player = iCol.GetComponent<CarController>();
        if (!!player)
        {
            cpm.notifyCP(this.gameObject);
        }
    }

    public Vector3 getSpawn()
    {
        return respawn_location.position;
    }

}
