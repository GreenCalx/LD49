using System.Collections.Generic;
using UnityEngine;

public class ObjectsEnabler : MonoBehaviour
{
    public List<ObjectDisabler> disablers;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider iCol)
    {
        CarController player = iCol.GetComponent<CarController>();
        if (!!player)
        {
            foreach (ObjectDisabler od in disablers)
                od.reenable();
        }
    }

    void OnTriggerStay(Collider iCol)
    {
        CarController player = iCol.GetComponent<CarController>();
        if (!!player)
        {
            foreach (ObjectDisabler od in disablers)
                od.reenable();
        }
    }
}
