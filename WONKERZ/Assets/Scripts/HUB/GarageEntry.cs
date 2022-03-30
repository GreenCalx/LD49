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
        if (playerInGarage)
        {
            if (Input.GetButtonDown("Submit"))
                openGarage();
        }
    }

    void OnTriggerEnter(Collider iCol)
    {
        playerCC = iCol.GetComponent<CarController>();
        // NOTE toffa : added check if playerInGarage because every collider will trigger, meaning we would be triggered multiple time on enter and on exit!
        if (playerCC && !playerInGarage)
        {
            playerRef = iCol.gameObject;
            playerInGarage = true;

            var SndMgr = GameObject.Find("SoundManager").GetComponent<SoundManager>();
            SndMgr.SwitchClip("garage");
        }
    }

    void OnTriggerExit(Collider iCol)
    {
        // NOTE toffa : added check if playerInGarage because every collider will trigger, meaning we would be triggered multiple time on enter and on exit!
        if (iCol.GetComponent<CarController>() && playerInGarage)
        {
            playerRef = null;
            playerCC = null;
            playerInGarage = false;
            closeGarage();


            var SndMgr = GameObject.Find("SoundManager").GetComponent<SoundManager>();
            SndMgr.SwitchClip("theme");
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
