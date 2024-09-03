using UnityEngine;
namespace Wonkerz {
/*
* A Camera for UI only scenes
*/
public class UICamera : GameCamera
{
    void Awake()
    {
        camType = CAM_TYPE.UI;
    }
    // Start is called before the first frame update
    void Start()
    {
        Access.CameraManager().changeCamera(camType, true);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void init()
    {
        
    }

    void OnDestroy() {
        // try to find a new camera with our type.
        // HACK:
        var cameras = GameObject.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var cam in cameras) {
            if (cam.name == "Main Camera" && cam != this.gameObject) {
                if (cam.GetComponent<UICamera>()) {
                    var uiCam = cam.GetComponent<UICamera>();
                    uiCam.enabled = true;
                    uiCam.cam.enabled = true;
                    uiCam.gameObject.SetActive(true);
                    uiCam.Start();
                    break;
                }
            }
        }
    }
}}
