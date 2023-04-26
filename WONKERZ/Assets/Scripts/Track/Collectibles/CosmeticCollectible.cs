/**
*   Can be : COLOR/Material, CAR PART
*/
using UnityEngine;

public class CosmeticCollectible : AbstractCollectible
{
    public string name= "";
    public COLORIZABLE_CAR_PARTS carPart;
    
    public Material matChange;
    public Mesh     modelChange;

    protected override void OnCollect()
    {
        Access.CollectiblesManager().applyCollectEffect(this);
    }
}