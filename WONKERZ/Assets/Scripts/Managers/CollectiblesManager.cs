using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class CollectiblesManager : MonoBehaviour
{
    [Serializable]
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

            public void removeFromJar(string iGameObjectName)
            {
                string scene_name = SceneManager.GetActiveScene().name;
                checkJar(scene_name);
                HashSet<string> collected;
                jar.TryGetValue(scene_name, out collected );
                if (collected.Count <= 0)
                    return;
                collected.Remove(iGameObjectName);
            }

            public bool IsInJar(T iAC, string iSceneName)
            {
                HashSet<string> collected;
                jar.TryGetValue(iSceneName, out collected );
                if (collected.Count <= 0)
                    return false;
                return collected.Contains(iAC.gameObject.name);
            }

            public HashSet<string> GetValues(string iSceneName)
            {
                HashSet<string> collected;
                jar.TryGetValue(iSceneName, out collected );
                return collected;
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
        public UniqueJar<CageKeyCollectible> CageKeyjar;
        public UniqueJar<GaragistCollectible> Garagistjar;

        // Infinites
        public int collectedNuts;

        public CollectibleJar()
        {
            collectedNuts   = 0;
            WONKERZjar      = new UniqueJar<CollectibleWONKERZ>();
            CageKeyjar      = new UniqueJar<CageKeyCollectible>();
            Garagistjar     = new UniqueJar<GaragistCollectible>();
        }

        public void collect(AbstractCollectible iAC)
        {
            if (iAC.collectibleType == AbstractCollectible.COLLECTIBLE_TYPE.UNIQUE)
            {
                if (iAC is CollectibleWONKERZ)
                {
                    WONKERZjar.addToJar(iAC as CollectibleWONKERZ);
                }
                else if (iAC is CageKeyCollectible)
                {
                    CageKeyjar.addToJar(iAC as CageKeyCollectible);
                }
                else if ( iAC is GaragistCollectible)
                {
                    Garagistjar.addToJar(iAC as GaragistCollectible);
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
    public COLLECT_MOD collectMod;
    public int collectModCombo;
    public CollectibleJar jar;
    public List<AbstractCollectible> allCollectiblesInCurrStage;

    ///
    [HideInInspector]
    public float currentTurbo;

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

    public void reset()
    {
        jar.collectedNuts = 0;
    }

    private void loadJar()
    {
        // TODO load the collected collectibles
    }

    public void saveJar()
    {
        // TODO save current jar status
    }

    public bool tryConsumeTurbo(float iAmount)
    { 
        if (currentTurbo < iAmount)
            return false;
        currentTurbo -= iAmount;
        Access.UITurboAndLifePool().updateTurboBar(currentTurbo);
        return true;
    }

    public bool tryConvertNutToTurbo()
    {
        if (jar.collectedNuts <= 0)
            return false;
         
        currentTurbo = ( currentTurbo >= 1f ) ? 1f : currentTurbo+nutTurboConvertValue;
        jar.collectedNuts--;

        Access.UITurboAndLifePool().updateTurboBar(currentTurbo);
        Access.UITurboAndLifePool().updateLifePool();

        return true;
    }

    public int getCollectedNuts()
    {
        return jar.collectedNuts;
    }

    public void loseNuts(int remove_n)
    {
        var clamp = jar.collectedNuts > 0;

        jar.collectedNuts-=remove_n;

        if (clamp)
            jar.collectedNuts = Mathf.Max(0, jar.collectedNuts);

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
                currentTurbo = ( currentTurbo >= 1f ) ? 1f : currentTurbo+nutTurboConvertValue;
                Access.UITurboAndLifePool().updateTurboBar(currentTurbo);
            }
            else
            {
                jar.collect(AC);
                Access.UITurboAndLifePool().updateLifePool();
            }
            allCollectiblesInCurrStage.Remove(AC);
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

    public bool hasWONKERZLetter(CollectibleWONKERZ.LETTERS iLetter, string iScene="")
    {
        string sceneName = (iScene=="") ? SceneManager.GetActiveScene().name : iScene;
        
        string letterAsStr = Enum.GetName(typeof(CollectibleWONKERZ.LETTERS), iLetter);
        
        HashSet<string> collected = jar.WONKERZjar.GetValues(sceneName);
        if ((collected==null) || (collected.Count <= 0))
            return false;

        foreach ( string e in jar.WONKERZjar.GetValues(sceneName) )
        {
            if ( letterAsStr == e )
                return true;
        }

        return false;
    }
    
    public bool hasCageKey(string iSceneName)
    {
        HashSet<string> collected = jar.CageKeyjar.GetValues(iSceneName);
        if ((collected==null) || (collected.Count <= 0))
            return false;

        return (collected.Count >= 1); // only one can be collected in a scene
    }

    public bool hasGaragist(string iSceneName)
    {
        HashSet<string> collected = jar.Garagistjar.GetValues(iSceneName);
        if ((collected==null) || (collected.Count <= 0))
            return false;

        return (collected.Count >= 1); // only one can be collected in a scene
    }

}
