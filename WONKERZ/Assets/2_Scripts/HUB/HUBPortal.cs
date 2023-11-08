using UnityEngine;
using Schnibble;

public class HUBPortal : MonoBehaviour
{
    public string PORTAL_SCENE_TARGET = "main";
    public bool invisibleLoading = false;

    private bool is_loading = false;
    [HideInInspector]
    public GameObject activeClippingPortal = null;

    // Start is called before the first frame update
    void Start()
    {
        is_loading = false;
        activeClippingPortal = null;
    }

    public void setActiveClippingPortal(GameObject iActiveClippingPortal)
    {
        activeClippingPortal = iActiveClippingPortal;
        this.Log("Active clipping plane : " + activeClippingPortal.name);
        ClippingPlane[] cps = GetComponentsInChildren<ClippingPlane>(true);
        for (int i = 0; i < cps.Length; i++)
        {
            cps[i].GetComponent<ClippingPlane>().enabled = (cps[i].name == activeClippingPortal.name);
        }
    }

    public void activatePortal()
    {
        if (!is_loading)
        {
            is_loading = true;
            if (invisibleLoading)
                Access.SceneLoader().loadScene(PORTAL_SCENE_TARGET, false, Constants.SN_BGLOADING);
            else
                Access.SceneLoader().loadScene(PORTAL_SCENE_TARGET);
        }
    }
}
