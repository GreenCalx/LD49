using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CageKeyCollectible : AbstractCollectible
{
    protected override void OnCollect()
    {
        Access.CollectiblesManager().applyCollectEffect(this);
        Destroy(gameObject);
    }
}
