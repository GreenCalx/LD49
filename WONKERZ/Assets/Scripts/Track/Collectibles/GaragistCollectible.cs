using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GaragistCollectible : AbstractCollectible
{
    protected override void OnCollect()
    {
        Access.CollectiblesManager().applyCollectEffect(this);
    }
}
