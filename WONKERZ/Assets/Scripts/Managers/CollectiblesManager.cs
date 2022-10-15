using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CollectiblesManager : MonoBehaviour
{
    public class CollectibleJar
    {
        public class UniqueJar<T> where T : AbstractCollectible
        {
            IDictionary<string,HashSet<string>> jar;

            public UniqueJar()
            {
                jar = new Dictionary<string, HashSet<string>>(0);
            }
            public void addToJar(T iAC)
            {
                string scene_name = SceneManager.GetActiveScene().name;
                checkJar(scene_name);
                HashSet<string> collected;
                jar.TryGetValue(scene_name, out collected );
                collected.Add(iAC.gameObject.name);
            }
            public void removeFromJar(T iAC)
            {
                string scene_name = SceneManager.GetActiveScene().name;
                checkJar(scene_name);
                HashSet<string> collected;
                jar.TryGetValue(scene_name, out collected );
                if (collected.Count <= 0)
                    return;
                collected.Remove(iAC.gameObject.name);
            }
            private void checkJar(string scene_name)
            {
                if (!jar.ContainsKey(scene_name))
                {
                    jar.Add( scene_name, new HashSet<string>());
                }
            }
        }

        // Uniques
        public UniqueJar<CollectibleWONKERZ> WONKERZjar;

        // Infinites
        public int collectedNuts;

        public CollectibleJar()
        {
            collectedNuts   = 0;
            WONKERZjar      = new UniqueJar<CollectibleWONKERZ>();
        }

        public void collect(AbstractCollectible iAC)
        {
            if (iAC.collectibleType == AbstractCollectible.COLLECTIBLE_TYPE.UNIQUE)
            {
                if (iAC is CollectibleWONKERZ)
                {
                    WONKERZjar.addToJar(iAC as CollectibleWONKERZ);
                }
                return;
            }
            /// Infinites collectibles
            if ( iAC is CollectibleNut )
            {
                collectedNuts++;
            }
        }
    }

    public enum COLLECT_MOD { HEAVEN=0, HELL=2 }

    [Header("Mandatory")]
    public GameObject nutCollectibleRef;
    public GameObject wonkerzCollectibleRef;

    [Header("Tweakables")]
    public Material heavenModeMat;
    public Material hellModeMat;
    [Range(0f,1f)]
    public float nutTurboConvertValue = 0.2f;
    [Range(0f,1f)]
    public float turboValueAtStart = 0f;
    [Range(1f,100f)]
    public float nutSpreadDistanceOnDamage = 10f;


    [Header("Internals")]
    private COLLECT_MOD collectMod;
    public int collectModCombo;
    public CollectibleJar jar;
    public List<AbstractCollectible> allCollectiblesInCurrStage;

    ///
    private float currentTurbo;

    void Awake()
    {
        allCollectiblesInCurrStage = new List<AbstractCollectible>();
    }

    // Start is called before the first frame update
    void Start()
    {
        collectModCombo = 0;
        jar = new CollectibleJar();
        loadJar();

        currentTurbo = turboValueAtStart;
    }

    private void loadJar()
    {
        // TODO load the collected collectibles
    }

    public void saveJar()
    {
        // TODO save current jar status
    }

    public int getCollectedNuts()
    {
        return jar.collectedNuts;
    }

    public void loseNuts(int remove_n)
    {
        jar.collectedNuts-=remove_n;
        if (jar.collectedNuts<0)
            jar.collectedNuts = 0;
        Access.UITurboAndLifePool().updateLifePool();
    }

    public int getCollectableCollectible<T>(GameObject iCollectibleHandle) where T : AbstractCollectible
    {
        return iCollectibleHandle.GetComponentsInChildren<T>(true).Length;
    }

    public void changeCollectMod( COLLECT_MOD iCollectMod )
    {
        if (iCollectMod != collectMod)
        {
            // break combo
            collectModCombo = 0;
            collectMod = iCollectMod;
            
            // update collectibles skin
            foreach(AbstractCollectible ac in allCollectiblesInCurrStage)
            {
                MeshRenderer mr = ac.gameObject.GetComponent<MeshRenderer>();
                if (iCollectMod == COLLECT_MOD.HELL)
                    mr.material = hellModeMat;
                else
                    mr.material = heavenModeMat;
            }

            // update player skin

        }
        collectModCombo++;
    }

    public void applyCollectEffect(AbstractCollectible AC)
    {
        if (AC is CollectibleNut)
        {
            if ( collectMod == COLLECT_MOD.HELL )
            {
                currentTurbo += ( currentTurbo >= 1f ) ? 0f : nutTurboConvertValue;
                Access.UITurboAndLifePool().updateTurboBar(currentTurbo);
            }
            else
            {
                jar.collect(AC);
                Access.UITurboAndLifePool().updateLifePool();
            }
        } 
        else
        {
            jar.collect(AC);
        }

    }

    public void subscribe(AbstractCollectible AC)
    {
        if (!allCollectiblesInCurrStage.Exists(x => x.gameObject.name == AC.gameObject.name))
            allCollectiblesInCurrStage.Add(AC);
    }
}
