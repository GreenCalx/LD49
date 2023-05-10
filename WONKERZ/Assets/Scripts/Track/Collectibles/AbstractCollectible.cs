using UnityEngine;


// TODO : Analyze memory leaks or if the GC does bad shiet with the created entities.
// Maybe give the lifecycle handle to the PlayerSkinManager ?
public static class CollectibleFactory<T> where T : AbstractCollectible
{
    public static T build( string iName, COLORIZABLE_CAR_PARTS iPart)
    {
        // Create empty
        GameObject retval = new GameObject("CC_"+iName);

        // Add comp
        CosmeticCollectible cc = retval.AddComponent<CosmeticCollectible>();

        // Decorate
        cc.name     = iName;
        cc.carPart  = iPart;

        return cc as T;
    }
}


public abstract class AbstractCollectible : MonoBehaviour
{
    public enum COLLECTIBLE_TYPE { INFINITE = 0, UNIQUE = 1 }
    public COLLECTIBLE_TYPE collectibleType = COLLECTIBLE_TYPE.INFINITE;

    protected abstract void OnCollect();

    void OnTriggerEnter(Collider iCollider)
    {
        if (Utils.isPlayer(iCollider.gameObject))
        {
            OnCollect();
        }
    }

    public void ForceCollect()
    {
        OnCollect();
    }
}
