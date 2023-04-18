using UnityEngine;
using Schnibble;

public class GarageEntry : MonoBehaviour, IControllable
{
    public GameObject garageUIRef;
    private GameObject garageUI;
    private bool playerInGarage;
    private bool garageOpened;
    public CarController playerCC;

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
        if (playerInGarage)
        {
            if (Entry.Inputs["Jump"].IsDown)
                openGarage();
        }
    }

    void OnTriggerEnter(Collider iCol)
    {
        // NOTE toffa : added check if playerInGarage because every collider will trigger, meaning we would be triggered multiple time on enter and on exit!
        if (!!iCol.GetComponent<CarController>() && !playerInGarage)
        {
            playerInGarage = true;
            playerCC = Access.Player();
            var SndMgr = Access.SoundManager();
            SndMgr.SwitchClip("garage");
        }
    }

    void OnTriggerExit(Collider iCol)
    {
        // NOTE toffa : added check if playerInGarage because every collider will trigger, meaning we would be triggered multiple time on enter and on exit!
        if (!!iCol.GetComponent<CarController>() && playerInGarage)
        {
            closeGarage();
            playerCC = null;
            playerInGarage = false;

            var SndMgr = Access.SoundManager();
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
        uig.onActivate.Invoke();
        
        
        playerCC.stateMachine.ForceState(playerCC.frozenState);
        
        garageOpened = true;
    }

    public void closeGarage()
    {
        if (!garageOpened)
            return;

        //Time.timeScale = 1; // unpause
        Destroy(garageUI);

        
        if (!!playerCC)
            playerCC.stateMachine.ForceState(playerCC.aliveState);
        else
        {
            CarController player = Access.Player();
            player.stateMachine.ForceState(player.aliveState);
        }

        garageOpened = false;
    }
}
