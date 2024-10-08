namespace Wonkerz
{
    public class UIGarage : UIGaragePanel
    {
        private GarageEntry garageEntry;

        protected override void Awake()
        {
            base.Awake();
            inputMgr = Access.managers.playerInputsMgr.player1;
        }

        override public void Deactivate()
        {
            base.Deactivate();
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
}
