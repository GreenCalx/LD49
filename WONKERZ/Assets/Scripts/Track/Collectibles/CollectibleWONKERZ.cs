using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleWONKERZ : AbstractCollectible
{
    public enum LETTERS { W, O, N, K, E, R, Z}

    public LETTERS currLetter;
    
    // Start is called before the first frame update
    void Start()
    {
        collectibleType = COLLECTIBLE_TYPE.UNIQUE;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void OnCollect()
    {
        gameObject.SetActive(false);
        //TODO : persist collected status
        Access.CollectiblesManager().applyCollectEffect(this);
    }
}
