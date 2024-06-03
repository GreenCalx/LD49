using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public enum ONLINE_COLLECTIBLES {
    NONE=0,
    NUTS=1
}

public class OnlineCollectible : NetworkBehaviour
{
    public bool IsCollectible = false;
    public float timeBeforeBeingCollectible = 0.2f;

    public ONLINE_COLLECTIBLES collectibleType;
    public int value = 1;
    [SyncVar]
    public bool collected = false;
    [Header("Effects")]
    public ParticleSystem self_onCollectPS;
    public AudioSource onCollectSound;

    private float elapsedTimeBeforeBeingCollectible = 0f;

    void Start()
    {
        elapsedTimeBeforeBeingCollectible =  0f;
        IsCollectible = false;
    }

    void Update()
    {
        if (!IsCollectible)
        {
            elapsedTimeBeforeBeingCollectible += Time.deltaTime;
            IsCollectible = elapsedTimeBeforeBeingCollectible >= timeBeforeBeingCollectible;
        }
    }

    [ServerCallback]
    void OnTriggerStay(Collider iCollider)
    {
        if (collected || !IsCollectible)
            return;

        AbstractCollector AC = iCollider.gameObject.GetComponent<AbstractCollector>();
        if (!!AC)
        {
            OnlinePlayerController locPlayer = AC.attachedOnlinePlayer;

            if (iCollider.transform.IsChildOf(locPlayer.transform))
                OnCollect(locPlayer);
            
            DestroySelf();
        }
    }

    protected void OnCollect(OnlinePlayerController iPlayer)
    {
        if (!iPlayer.isServer)
        {
            iPlayer.bag.CmdCollect(this);
        }
        else
        {
            iPlayer.bag.AsServerCollect(this);
        }

        if (!!onCollectSound)
            Schnibble.Utils.SpawnAudioSource(onCollectSound, transform);
        


        collected = true;
    }

        // destroy for everyone on the server
        [Server]
        void DestroySelf()
        {
            RpcCollectFX();
            NetworkServer.Destroy(gameObject);
        }

        [ClientRpc]
        public void RpcCollectFX()
        {
            if (!!self_onCollectPS)
            {
                self_onCollectPS.gameObject.SetActive(true);
                self_onCollectPS.transform.parent = null;
                //collectPS.transform.position = transform.position;
                self_onCollectPS.Play();
                Destroy(self_onCollectPS.gameObject, self_onCollectPS.main.duration);
            }
        }
}
