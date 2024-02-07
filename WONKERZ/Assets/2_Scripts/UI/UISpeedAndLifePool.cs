using UnityEngine;
using UnityEngine.UI;
using Schnibble;
using TMPro;

public class UISpeedAndLifePool : MonoBehaviour
{
    public TMPro.TextMeshProUGUI    speedText;
    public Image                    speedBar;
    public TextMeshProUGUI          lifePool;

    public Image            cpImageFilled;
    public TextMeshProUGUI  nAvailablePanels;
    public TextMeshProUGUI idOfLastCPTriggered;

    private PlayerController player;
    private CollectiblesManager CM;

    void Start()
    {
        player = Access.Player();
        CM = Access.CollectiblesManager();

        updateLastCPTriggered("x");
        cpImageFilled.fillAmount = 0f;

        updateSpeedCounter();
        updateLifePool();
    }

    void Update()
    {
        updateSpeedCounter();
        updateLifePool();
    }

    public void updateSpeedCounter()
    {
        var PlayerVelocity = player.rb.velocity.magnitude;
        CarController cc = player.car;
        float max_speed = cc.maxTorque;

        // Update Bar
        float bar_percent = Mathf.Clamp(PlayerVelocity / max_speed, 0f, max_speed);
        speedBar.fillAmount = bar_percent;

        // Update Text
        string lbl = ((int)PlayerVelocity).ToString();
        speedText.SetText(lbl);
    }

    public void updateLifePool()
    {
        int n_nuts = CM.getCollectedNuts();
        lifePool.text = (n_nuts > 0) ? n_nuts.ToString() : "0";
    }

    public void updateAvailablePanels(int iVal)
    {
        int val = (iVal > 0) ? iVal : 0 ;
        
        string str = (val < 999) ? val.ToString() : "inf";

        nAvailablePanels.text = str;
    }

    public void updateLastCPTriggered(string iTxt)
    {
        idOfLastCPTriggered.text = iTxt;
    }

    public void updateCPFillImage(float iVal)
    {
        cpImageFilled.fillAmount = iVal;
    }
    public void updatePanelOnRespawn()
    {

    }
}
