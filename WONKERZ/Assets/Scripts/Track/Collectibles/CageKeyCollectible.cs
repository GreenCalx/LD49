
public class CageKeyCollectible : AbstractCollectible
{
    protected override void OnCollect()
    {
        Access.CollectiblesManager().applyCollectEffect(this);
        Destroy(gameObject);
    }
}
