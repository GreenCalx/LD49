using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

using Wonkerz;

public class OnlineShopItem : MonoBehaviour
{
    [Header("Handles")]
    public Image displayedImage;
    public TextMeshProUGUI costLabel;
    public Transform costUIHandle;

    
    [Header("Internal")]
    public int cost;
    public OnlineCollectible item;
    public bool display = false;
    public bool refreshed = false;
    public bool isSelected = false;

    // Start is called before the first frame update
    void Start()
    {
        Refresh();
    }

    // Update is called once per frame
    void Update()
    {
        if (!refreshed)
            Refresh();

    }

    private void Refresh()
    {

        if (display)
        {
            if (item==null)
                return;

            displayedImage.sprite = item.positiveStatTex;
            displayedImage.color = Color.white;
            costLabel.text = cost.ToString();
            costUIHandle.gameObject.SetActive(true);

        } else
        {
            displayedImage.sprite = null;
            displayedImage.color = new Color( 0xFF, 0xFF, 0xFF, 0x00 );
            costUIHandle.gameObject.SetActive(false);

        }

        refreshed = true;
    }
}
