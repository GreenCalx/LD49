
public class GaragistCollectible : AbstractCollectible
{
    protected override void OnCollect()
    {
        Access.CollectiblesManager().applyCollectEffect(this);
    }
}
