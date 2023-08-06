using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UITurboAndSaves : MonoBehaviour
{

    public Image            turboBar;
    public Image            cpImageFilled;
    public GameObject turboRefilAnim;

    public TextMeshProUGUI nAvailablePanels;
    //public TextMeshProUGUI nPanelRespawn;

    public TextMeshProUGUI idOfLastCPTriggered;

    // Start is called before the first frame update
    void Start()
    {
        updateTurboBar();
        updateLastCPTriggered("x");
        cpImageFilled.fillAmount = 0f;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void updateAvailablePanels(int iVal)
    {
        int val = (iVal > 0) ? iVal : 0 ;
        
        string str = (val < 999) ? val.ToString() : "inf";

        nAvailablePanels.text = str;
    }

    public void updatePanelOnRespawn()
    {

    }

    public void startTurboRefilAnim(bool v)
    {
        turboRefilAnim.SetActive(v);
    }

    public void updateTurboBar()
    {
        PlayerController pc = Access.Player();
        turboBar.fillAmount = pc.turbo.current / pc.turbo.max ;
    }

    public void updateLastCPTriggered(string iTxt)
    {
        idOfLastCPTriggered.text = iTxt;
    }

    public void updateCPFillImage(float iVal)
    {
        cpImageFilled.fillAmount = iVal;
    }
}
