using UnityEngine;

public class CageKeyCollectible : AbstractCollectible
{
    public string trackname = "";
    void Start()
    {
        if (Access.CollectiblesManager().hasCageKey(trackname))
        {
            gameObject.SetActive(false);
        }
    }

    protected override void OnCollect()
    {
        Access.CollectiblesManager().applyCollectEffect(this);
        Destroy(gameObject);
    }
}
