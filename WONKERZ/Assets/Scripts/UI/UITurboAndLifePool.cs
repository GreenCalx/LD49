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
    // Start is called before the first frame update
    void Start()
    {
        updateLifePool();
        updateTurboBar(Access.CollectiblesManager().turboValueAtStart);
    }

    // Update is called once per frame
    void Update()
    {
        
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
