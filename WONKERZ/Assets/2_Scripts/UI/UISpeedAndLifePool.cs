using UnityEngine;
using UnityEngine.UI;
using Schnibble;
using TMPro;

public class UISpeedAndLifePool : MonoBehaviour
{
    public TMPro.TextMeshProUGUI    speedText;
    public Image                    speedBar;
    public TextMeshProUGUI          lifePool;

    void Start()
    {
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
        PlayerController pc = Access.Player();
        var PlayerVelocity = pc.rb.velocity.magnitude;
        CarController cc = pc.car;
        float max_speed = cc.maxTorque;

        // Update Bar
        float bar_percent = Mathf.Clamp(PlayerVelocity / max_speed, 0f, max_speed);
        speedBar.fillAmount = bar_percent;

        // Update Text
        string lbl = ((int)PlayerVelocity).ToString();
        lbl += " KMH";
        speedText.SetText(lbl);
    }

    public void updateLifePool()
    {
        int n_nuts = Access.CollectiblesManager().getCollectedNuts();
        lifePool.text = (n_nuts > 0) ? n_nuts.ToString() : "0";
    }
}
