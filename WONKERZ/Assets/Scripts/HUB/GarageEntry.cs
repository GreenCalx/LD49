using UnityEngine;
using Schnibble;

public class GarageEntry : MonoBehaviour, IControllable
{
    public GameObject garageUIRef;
    public PlayerDetector detector;

    private GameObject garageUI;
    private bool playerInGarage;
    private bool garageOpened;
    
    [HideInInspector]
    public PlayerController player;

    // Start is called before the first frame update
    void Start()
    {
        playerInGarage = false;
        Utils.attachControllable<GarageEntry>(this);
    }

    void OnDestroy()
    {
        Utils.detachControllable<GarageEntry>(this);
    }

    void IControllable.ProcessInputs(InputManager.InputData Entry)
    {
        if (detector.playerInRange)
        {
            if (Entry.Inputs[(int)GameInputsButtons.Jump].IsDown)
            openGarage();
        }
    }


    public void openGarage()
    {
        if (garageOpened)
            return;

        player = Access.Player();

        //Time.timeScale = 0; // pause
        garageUI = Instantiate(garageUIRef);
        garageUI.name = Constants.GO_UIGARAGE;

        UIGarage uig = garageUI.GetComponent<UIGarage>();
        uig.setGarageEntry(this.GetComponent<GarageEntry>());
        uig.onActivate.Invoke();

        player.Freeze();

        var SndMgr = Access.SoundManager();
        SndMgr.SwitchClip("garage");

        garageOpened = true;
    }

    public void closeGarage()
    {
        if (!garageOpened)
            return;

        //Time.timeScale = 1; // unpause
        Destroy(garageUI);


        if (!!player)
        player.UnFreeze();
        else
        {
            PlayerController player = Access.Player();
            if (player) player.UnFreeze();
            else
            this.LogWarn("No player could be found!");
        }

        var SndMgr = Access.SoundManager();
        SndMgr.SwitchClip("theme");

        garageOpened = false;
    }
}
