using UnityEngine;

public class GarageEntry : MonoBehaviour,IControllable
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
        Utils.attachControllable<GarageEntry>(this);
    }

    void OnDestroy() {
        Utils.detachControllable<GarageEntry>(this);
    }

    void IControllable.ProcessInputs(InputManager.InputData Entry){
        if (playerInGarage)
        {
            if (Entry.Inputs["Jump"].IsDown)
                openGarage();
        }
    }

    // Update is called once per frame
    void Update()
    {
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
        //Time.timeScale = 0; // pause
        garageUI = Instantiate(garageUIRef);
        garageUI.name = Constants.GO_UIGARAGE;

        UIGarage uig = garageUI.GetComponent<UIGarage>();
        uig.setGarageEntry(this.GetComponent<GarageEntry>());
        garageOpened = true;

        GameObject.Find(Constants.GO_MANAGERS).GetComponent<InputManager>().SetUnique(uig as IControllable);
        //Utils.detachControllable<CarController>(playerCC);
        playerCC.isFrozen = true;
    }

    public void closeGarage()
    {
        if (!garageOpened)
            return;


        GameObject.Find(Constants.GO_MANAGERS).GetComponent<InputManager>().UnsetUnique(garageUI.GetComponent<UIGarage>() as IControllable);

        //Time.timeScale = 1; // unpause
        Destroy(garageUI);

        //Utils.attachControllable<CarController>(playerCC);
        if (!!playerCC)
            playerCC.isFrozen = false;
        else
            Utils.getPlayerRef().GetComponent<CarController>().isFrozen = false;
        garageOpened = false;
    }
}
