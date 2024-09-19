using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Mirror;

using Schnibble;

public enum ONLINE_COLLECTIBLES {
    NONE=0,
    NUTS=1,
    ACCEL=2,
    MAX_SPEED=3,
    SPRINGS=4,
    TURN=5,
    TORQUEFORCE=6,
    WEIGHT=7,
    KLANCE_POWER = 8,
    PLAUNCHER = 9
}

public class OnlineCollectible : NetworkBehaviour
{
    [SyncVar]
    public bool IsCollectible = false;
    public float timeBeforeBeingCollectible = 0.2f;

    public ONLINE_COLLECTIBLES collectibleType;
    public Sprite positiveStatTex; // used by onlineshopitem
    public Sprite negativeStatTex;
    [SyncVar]
    public int value = 1;
    [SyncVar]
    public int shopCost = 3;
    [SyncVar]
    public bool collected = false;
    [Header("Effects")]
    public ParticleSystem self_onCollectPS;
    public AudioSource onCollectSound;

    private float elapsedTimeBeforeBeingCollectible = 0f;

    void Start()
    {
        if (isClientOnly)
        { return; }

        elapsedTimeBeforeBeingCollectible =  0f;
        IsCollectible = false;
    }

    void Update()
    {
        if (isClientOnly)
        { return; }

        if (!IsCollectible)
        {
            elapsedTimeBeforeBeingCollectible += Time.deltaTime;
            IsCollectible = elapsedTimeBeforeBeingCollectible >= timeBeforeBeingCollectible;
        }
    }



    [Server]
    public void SetAsNegative()
    {
        StartCoroutine(SetNegativeCo());
    }

    IEnumerator SetNegativeCo()
    {
        while (!isServer) // not spawned yet on server
        {
            yield return null;
        }

        value = -1;
        RpcChangeImageToNegative();

        Image img = GetComponentInChildren<Image>();
        if (!!img)
        {
            img.sprite = negativeStatTex;
        }
    }

    [ClientRpc]
    public void RpcChangeImageToNegative()
    {
        Image img = GetComponentInChildren<Image>();
        if (!!img)
        {
            img.sprite = negativeStatTex;
        }

        self_onCollectPS.textureSheetAnimation.SetSprite(0, negativeStatTex);
    }

    void OnTriggerStay(Collider iCollider)
    {
        if (!isServer)
            return;

        if (collected || !IsCollectible)
            return;

        AbstractCollector AC = iCollider.gameObject.GetComponent<AbstractCollector>();
        if (!!AC)
        {
            OnlinePlayerController locPlayer = AC.attachedOnlinePlayer;
            if (locPlayer == null) {
                this.LogError("locPlayer is null.");
            }


            if (locPlayer.bag.HasAPowerEquipped && IsPowerCollectible())
                return; // player already has a power we don't collect

            if (iCollider.transform.IsChildOf(locPlayer.transform))
                OnCollect(locPlayer);

            DestroySelf();
        }
    }

    public void OnCollect(OnlinePlayerController iPlayer)
    {
        if (!iPlayer.isServer)
        {
            iPlayer.bag.CmdCollect(this);
        }
        else
        {
            iPlayer.bag.AsServerCollect(this);
        }

        collected = true;
    }

        // destroy for everyone on the server
        [Server]
        void DestroySelf()
        {
            if (!isServerOnly)
                CollectFX();
            RpcCollectFX();
            NetworkServer.Destroy(gameObject);
        }

        public void CollectFX()
        {
            if (!!onCollectSound)
                Schnibble.Utils.SpawnAudioSource(onCollectSound, transform);

            if (!!self_onCollectPS)
            {
                self_onCollectPS.gameObject.SetActive(true);
                self_onCollectPS.transform.parent = null;
                //collectPS.transform.position = transform.position;
                self_onCollectPS.Play();
                Destroy(self_onCollectPS.gameObject, self_onCollectPS.main.duration);
            }
        }

        [ClientRpc]
        public void RpcCollectFX()
        {
            CollectFX();
        }

        private bool IsPowerCollectible()
        {
            return (collectibleType==ONLINE_COLLECTIBLES.KLANCE_POWER) ||
                    (collectibleType==ONLINE_COLLECTIBLES.PLAUNCHER);
        }
}
