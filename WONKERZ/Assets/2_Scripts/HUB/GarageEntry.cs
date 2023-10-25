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

    public Camera garageCamera;

    // Start is called before the first frame update
    void Start()
    {
        playerInGarage = false;
        Access.PlayerInputsManager().player1.Attach(this as IControllable);
    }

    void OnDestroy()
    {
        Access.PlayerInputsManager().player1.Detach(this as IControllable);
    }

    void IControllable.ProcessInputs(InputManager currentMgr, GameInput[] Entry)
    {
        if (detector.playerInRange)
        {
            if ((Entry[(int)PlayerInputs.InputCode.UIValidate] as GameInputButton).GetState().down)
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

        garageCamera.gameObject.SetActive(true);

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

        garageCamera.gameObject.SetActive(false);

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
