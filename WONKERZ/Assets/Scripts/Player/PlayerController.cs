using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;

public class PlayerController : MonoBehaviour
{
    CarController car;
    void Awake()
    {
        car = GetComponent<CarController>();
        Utils.attachControllable<CarController>(car);
    }

    void OnDestroy()
    {
        Utils.detachControllable<CarController>(car);
    }
    // Start is called before the first frame update
    void Start()
    {
        car = GetComponent<CarController>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
