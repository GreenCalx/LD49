
public class UIGarage : UIGaragePanel
{
    private GarageEntry garageEntry;

    protected override void Awake()
    {
        base.Awake();
        inputMgr = Access.InputManager();
    }

    override public void deactivate()
    {
        base.deactivate();
        garageEntry.closeGarage();
    }

    public GarageEntry getGarageEntry()
    {
        return garageEntry;
    }

    public void setGarageEntry(GarageEntry iGE)
    {
        garageEntry = iGE;
    }

    public void closeGarage()
    {
        garageEntry.closeGarage();
    }
}
