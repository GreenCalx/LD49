
namespace Wonkerz {
    public class GaragistCollectible : AbstractCollectible
    {
        public new string name = "Garager";
    
        protected override void OnCollect()
        {
            Access.managers.collectiblesMgr.applyCollectEffect(this);
        }
    }
}
