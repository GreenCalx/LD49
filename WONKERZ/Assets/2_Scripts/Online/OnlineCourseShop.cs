using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using Mirror;
using Wonkerz;

public class OnlineCourseShop : NetworkBehaviour
{
    [Header("Handles")]
    //public Dictionary<OnlineCollectible, int> itemCostDict;
    public List<OnlineCollectible> items;
    public List<OnlineShopItem> worldShopItems;
    public CameraFocusable self_focusable;
    public Animator self_Animator;
    [Header("Internals")]
    
    [SyncVar]
    public OnlinePlayerController playerInShop;
    public int selected_idx;
    private Vector3 focusableInitPosition = Vector3.zero;

    // Animations
    private readonly string PointLeftParm = "PointLeftItem"; // item1
    private readonly string PointRightParm = "PointRightItem"; // item0
    private readonly string ShopOpenedParm = "ShopOpened";
    private readonly string ExitShopTrigg = "ExitShop";

    public void Start()
    {
        focusableInitPosition = self_focusable.transform.position;
    }

    public void Update()
    {
        if (!!self_focusable && self_focusable.isFocus)
        {
            if (self_Animator)
            {
                self_Animator.SetBool(ShopOpenedParm, true);
            }
        }
    }

    public void EnterShop()
    {
        selected_idx = 0;
        
        DisplayShop();
    }

    public void QuitShop()
    {

        if (self_Animator)
        {
            self_Animator.SetTrigger(ExitShopTrigg);
            self_Animator.SetBool(ShopOpenedParm, false);
            self_Animator.SetBool(PointLeftParm, false);
            self_Animator.SetBool(PointRightParm, false);
        }

        foreach(OnlineShopItem shopItem in worldShopItems)
        {
            shopItem.display = false;
            shopItem.refreshed = false;
        }

        self_focusable.transform.position = focusableInitPosition;
        self_focusable.callbackOnAction.RemoveAllListeners();
        self_focusable.callbackOnAction.AddListener(EnterShop);
        self_focusable.actionName = "SHOP";
    }

    public void DisplayShop()
    {
        int shopitem_idx = 0;

        foreach (OnlineCollectible item in items)
        {
            OnlineShopItem shopItem = worldShopItems[shopitem_idx];
            shopItem.item = item;
            shopItem.cost = item.shopCost;

            shopItem.display = true;

            shopItem.refreshed = false;

            shopitem_idx++;
            if (shopitem_idx > worldShopItems.Count)
                break;
        }

        selected_idx = 0;
        UpdateSelectedItem();

        self_focusable.callbackOnAction.RemoveAllListeners();
        self_focusable.callbackOnAction.AddListener(TryBuyItem);
        self_focusable.actionName = "BUY";

        playerInShop = NetworkRoomManagerExt.singleton.onlineGameManager.localPlayer;
    }

    public void UpdateSelectedItem()
    {
        OnlineShopItem shopItem = worldShopItems[selected_idx];
        shopItem.isSelected = true;
        self_focusable.transform.position = shopItem.transform.position;

        if (self_Animator)
        {
            self_Animator.SetBool(PointRightParm, (selected_idx==0));
            self_Animator.SetBool(PointLeftParm, (selected_idx==1));
        }
    }

    public void TryBuyItem()
    {
        OnlineShopItem shopItem = worldShopItems[selected_idx];
        if (playerInShop.bag.nuts  >= shopItem.cost)
        {
            // can buy
            playerInShop.bag.nuts -= shopItem.cost;
            //shopItem.item.OnCollect(playerInShop);

            GameObject NewObject = Instantiate(shopItem.item.gameObject);
            NewObject.transform.position = playerInShop.self_PlayerController.GetTransform().position;
            NewObject.transform.parent = null;

            NetworkServer.Spawn(NewObject);

        } else {
            // cannot buy
        }
    }

}
