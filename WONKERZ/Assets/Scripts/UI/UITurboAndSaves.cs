using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UITurboAndSaves : MonoBehaviour
{

    public Image            turboBar;

    public TextMeshProUGUI nAvailablePanels;
    //public TextMeshProUGUI nPanelRespawn;

    public TextMeshProUGUI idOfLastCPTriggered;

    // Start is called before the first frame update
    void Start()
    {
        updateTurboBar(Access.CollectiblesManager().turboValueAtStart);
        updateLastCPTriggered("x");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void updateAvailablePanels(int iVal)
    {
        int val = (iVal > 0) ? iVal : 0 ;
        string str = (val < 999) ? val.ToString() : "∞";

        nAvailablePanels.text = str;
    }

    public void updatePanelOnRespawn()
    {

    }

    public void updateTurboBar(float turboValue)
    {
        turboBar.fillAmount = turboValue;
    }

    public void updateLastCPTriggered(string iTxt)
    {
        idOfLastCPTriggered.text = iTxt;
    }
}
