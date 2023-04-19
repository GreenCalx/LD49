
public class GaragistCollectible : AbstractCollectible
{
    public string name = "Garager";
    
    protected override void OnCollect()
    {
        Access.CollectiblesManager().applyCollectEffect(this);
    }
}
