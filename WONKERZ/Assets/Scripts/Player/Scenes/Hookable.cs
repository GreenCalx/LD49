using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hookable : MonoBehaviour, IControllable
{
    private CarController   ccPlayer;
    public GameObject       hook;
    public Vector3          D;

    // Start is called before the first frame update
    void Start()
    {
        Utils.attachControllable<Hookable>(this);
        ccPlayer = null;
        hook.SetActive(false);
    }

    private void OnDestroy()
    {
        Utils.detachControllable<Hookable>(this);
    }

    void IControllable.ProcessInputs(InputManager.InputData Entry)
    {
        hook.SetActive(Entry.Inputs["Grapin"].Down);
        if (!!ccPlayer)
            ccPlayer.IsHooked = Entry.Inputs["Grapin"].Down;
    }

    // Update is called once per frame
    void Update()
    {
        if (!!ccPlayer && ccPlayer.IsHooked)
        {
            D = (transform.position - ccPlayer.transform.position);
            hook.transform.position = transform.position - D / 2;
            hook.transform.localScale = new Vector3(1, D.magnitude / 2, 1) / 10;
            hook.transform.rotation = Quaternion.FromToRotation(transform.up, D);
        }
    }

    public void OnTriggerEnter(Collider iCol)
    {
        if (null==ccPlayer)
            ccPlayer = iCol.GetComponent<CarController>();
    }
    public void OnTriggerStay(Collider iCol)
    {
        if (null==ccPlayer)
            ccPlayer = iCol.GetComponent<CarController>();
    }
    public void OnTriggerExit(Collider iCol)
    {
        if (!!ccPlayer && !ccPlayer.IsHooked)
            ccPlayer = iCol.GetComponent<CarController>();
    }
}
