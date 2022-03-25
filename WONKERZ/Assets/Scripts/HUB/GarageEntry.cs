using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarageEntry : MonoBehaviour
{
    public GameObject garageUIRef;
    private GameObject garageUI;
    private bool playerInGarage;
    private bool garageOpened;
    public GameObject playerRef;
    public CarController playerCC;

    // Start is called before the first frame update
    void Start()
    {
        playerInGarage = false;
    }

    // Update is called once per frame
    void Update()
    {
        if ( playerInGarage )
        {
            if (Input.GetButtonDown("Submit"))
                openGarage();
        }
    }

    void OnTriggerEnter(Collider iCol)
    {
        playerCC = iCol.GetComponent<CarController>();
        if (playerCC)
        {
            playerRef = iCol.gameObject;
            playerInGarage = true;
        }
    }

    void OnTriggerExit(Collider iCol)
    {
        if (iCol.GetComponent<CarController>())
        {
            playerRef = null;
            playerCC = null;
            playerInGarage = false;
            closeGarage();
        }
    }

    public void openGarage()
    {
        if (garageOpened)
            return;
        Time.timeScale = 0; // pause
        garageUI = Instantiate(garageUIRef);

        UIGarage uig = garageUI.GetComponent<UIGarage>();
        uig.setGarageEntry(this.GetComponent<GarageEntry>());
        garageOpened = true;
    }

    public void closeGarage()
    {
        if (!garageOpened)
            return;
        Time.timeScale = 1; // unpause
        Destroy(garageUI);

        garageOpened = false;
    }
}
