using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UITurboAndLifePool : MonoBehaviour
{

    public Image            turboBar;
    public TextMeshProUGUI  lifePool;

    public TextMeshProUGUI nPanelUsed;
    public TextMeshProUGUI nPanelRespawn;

    // Start is called before the first frame update
    void Start()
    {
        updateLifePool();
        updateTurboBar(Access.CollectiblesManager().turboValueAtStart);
        updatePanelUsed(0);
        updatePanelRespawn(0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void updatePanelUsed(int iPanelUsed)
    {
        nPanelUsed.text = (iPanelUsed > 0) ? iPanelUsed.ToString() : "0";
    }

    public void updatePanelRespawn(int iPanelRespawn)
    {
        nPanelRespawn.text = (iPanelRespawn > 0) ? iPanelRespawn.ToString() : "0";
    }

    public void updateLifePool()
    {
        int n_nuts = Access.CollectiblesManager().getCollectedNuts();
        lifePool.text = (n_nuts > 0) ? n_nuts.ToString() : "0";
    }

    public void updateTurboBar(float turboValue)
    {
        turboBar.fillAmount = turboValue;
    }
}
