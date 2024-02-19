using UnityEngine;
using Schnibble;
using System.Collections.Generic;
using System.Collections; 

using Schnibble.Managers;

public class GarageEntry : MonoBehaviour, IControllable
{
    [Header("Mand")]
    public GameObject garageUIRef;
    public List<GameObject> self_worldSpaceHints;
    public PlayerDetector detector;
    [Header("Anims")]
    public List<GameObject> garageDoors;
    public float garageDoorsYDelta = 40f;
    [Header("Lights")]
    public GameObject sun;
    public GameObject garageMainLight;
    public List<GameObject> garageLights;

    private GameObject garageUI;
    private bool playerInGarage;
    private bool garageOpened;
    
    [HideInInspector]
    public PlayerController player;

    public CinematicCamera garageCamera;

    // Start is called before the first frame update
    void Start()
    {
        playerInGarage = false;
        Access.PlayerInputsManager().player1.Attach(this as IControllable);
    }

    void OnDestroy()
    {
        Access.PlayerInputsManager()?.player1.Detach(this as IControllable);
    }

    void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
    {
        if (detector.playerInRange)
        {
            if ((Entry.Get((int)PlayerInputs.InputCode.UIValidate) as GameInputButton).GetState().down)
            openGarage();
        }
    }


    public void openGarage()
    {
        if (garageOpened)
        return;

        foreach(GameObject go in garageDoors)
        {
            StartCoroutine(closeDoor(go));
        }
        turnOnLights();

        player = Access.Player();

        //Time.timeScale = 0; // pause
        garageUI = Instantiate(garageUIRef);
        garageUI.name = Constants.GO_UIGARAGE;

        //garageCamera.launch();
        ManualCamera manCam = Access.CameraManager().active_camera.GetComponent<ManualCamera>();
        if (manCam)
        {
            manCam.forceAngles(true, new Vector2(0f, 90f));
        }

        Access.UISpeedAndLifePool().gameObject.SetActive(false);

        detector.gameObject.SetActive(false);
        foreach(GameObject to_deactivate in self_worldSpaceHints)
        {
            to_deactivate.SetActive(false);
        }
        

        UIGarage uig = garageUI.GetComponent<UIGarage>();
        uig.setGarageEntry(this.GetComponent<GarageEntry>());
        uig.onActivate.Invoke();

        player.transform.position = transform.position;
        player.transform.rotation = Quaternion.identity;
        player.Freeze();

        var SndMgr = Access.SoundManager();
        SndMgr.SwitchClip("garage");

        garageOpened = true;
    }

    public void closeGarage()
    {
        if (!garageOpened)
        return;

        foreach(GameObject go in garageDoors)
        {
            StartCoroutine(openDoor(go));
        }
        turnOffLights();
        
        //garageCamera.end();
        ManualCamera manCam = Access.CameraManager().active_camera.GetComponent<ManualCamera>();
        if (manCam)
        {
            manCam.forceAngles(false, new Vector2(0f, 0f));
            manCam.resetView();
        }

        //Time.timeScale = 1; // unpause
        Destroy(garageUI);

        Access.UISpeedAndLifePool().gameObject.SetActive(true);
        
        foreach(GameObject to_reactivate in self_worldSpaceHints)
        {
            to_reactivate.SetActive(true);
        }
        detector.gameObject.SetActive(true);

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

    IEnumerator closeDoor(GameObject iGO)
    {
        Vector3 target_pos = iGO.transform.position + new Vector3(0f, garageDoorsYDelta, 0f);

        float elapsedTime = 0;
        float waitTime = 1.5f;
        while (elapsedTime < waitTime)
        {
            iGO.transform.position = Vector3.Lerp(iGO.transform.position, target_pos, (elapsedTime / waitTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }  
    }

    IEnumerator openDoor(GameObject iGO)
    {
        Vector3 target_pos = iGO.transform.position - new Vector3(0f, garageDoorsYDelta, 0f);

        float elapsedTime = 0;
        float waitTime = 1.5f;
        while (elapsedTime < waitTime)
        {
            iGO.transform.position = Vector3.Lerp(iGO.transform.position, target_pos, (elapsedTime / waitTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }  
    }

    private void turnOffLights()
    {
        foreach (GameObject l in garageLights)
        {
            l.SetActive(false);
        }
        sun.SetActive(true);
        // update toon pipeline mainLight?
        Access.CameraManager().changeMainLight(sun.GetComponent<Light>());
    }
    private void turnOnLights()
    {
        foreach (GameObject l in garageLights)
        {
            l.SetActive(true);
        }
        sun.SetActive(false);

        // update toon pipeline mainLight?
        Access.CameraManager().changeMainLight(garageMainLight.GetComponent<Light>());
    }
}
 
