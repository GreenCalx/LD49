using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBountyUnlocked : MonoBehaviour
{
    public GameObject globalShowHideHandle;
    public TMPro.TextMeshProUGUI bounty_name;
    public TMPro.TextMeshProUGUI bounty_reward;
    public float FULL_DURATION = 3f;

    private bool is_enabled = false;
    private float display_start_time;

    // Start is called before the first frame update
    void Start()
    {
        disable();
    }

    void Update()
    {
        if (is_enabled)
            if ((Time.time - display_start_time) >= FULL_DURATION)
                disable();
    }

    public void disable()
    {
        globalShowHideHandle.SetActive(false);
        is_enabled = false;
    }

    public void enable()
    {
        globalShowHideHandle.SetActive(true);
        is_enabled = true;
    }

    public void display(BountyArray.AbstractBounty iTEB)
    {
        enable();
        bounty_name.text    = iTEB.name;

        string reward_txt = "";
        foreach ( CosmeticElement ce in iTEB.GetAttachedCosmetics())
        { reward_txt += ce.name; reward_txt += '\n'; }
        bounty_reward.text  = reward_txt;
        display_start_time = Time.time;
    }

}
