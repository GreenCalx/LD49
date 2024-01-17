using UnityEngine;
using UnityEngine.UI;
using Schnibble;
using TMPro;

public class UISpeedAndLifePool : MonoBehaviour
{
    public TMPro.TextMeshProUGUI    speedText;
    public Image                    speedBar;
    public TextMeshProUGUI          lifePool;
    private PlayerController player;
    private CollectiblesManager CM;

    void Start()
    {
        player = Access.Player();
        CM = Access.CollectiblesManager();

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
}
