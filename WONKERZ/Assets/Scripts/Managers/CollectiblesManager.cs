using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CollectiblesManager : MonoBehaviour
{
    public class CollectibleJar
    {
        // List of GO's names
        public HashSet<string> collected_hub_nuts;
        public HashSet<string> collected_desert_nuts;

        public CollectibleJar()
        {
            collected_hub_nuts      = new HashSet<string>();
            collected_desert_nuts   = new HashSet<string>();
        }

        public void collectNut(CollectibleNut iNut)
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name==Constants.SN_HUB)
            {
                collected_hub_nuts.Add(iNut.gameObject.name);
                Debug.Log("Nut collected in hub!");
            } 
            else if (scene.name==Constants.SN_DESERT)
            {
                collected_desert_nuts.Add(iNut.gameObject.name);
                Debug.Log("Nut collected in desert!");
            }
        }
    }

    public enum COLLECT_MOD { NEUTRAL=0, HEAVEN=1, HELL=2 }
    private COLLECT_MOD collectMod;
    public int collectModCombo;
    public CollectibleJar jar;
    public List<AbstractCollectible> allCollectiblesInCurrStage;

    [Header("Tweakables")]
    public Material heavenModeMat;
    public Material hellModeMat;

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
    }

    private void loadJar()
    {
        // TODO load the collected collectibles
    }

    public void saveJar()
    {
        // TODO save current jar status
    }

    public void addToJar(AbstractCollectible iCollectible)
    {
        if (iCollectible is CollectibleNut)
        {
            jar.collectNut(iCollectible as CollectibleNut);
        }
    }

    public int getCollectedNuts(string iSceneName)
    {
        switch(iSceneName)
        {
            case Constants.SN_HUB:
                return jar.collected_hub_nuts.Count;
                break;
            case Constants.SN_DESERT:
                return jar.collected_desert_nuts.Count;
                break;
            default:
            return -1;
                break;
        }
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

    }

    public void subscribe(AbstractCollectible AC)
    {
        if (!allCollectiblesInCurrStage.Exists(x => x.gameObject.name == AC.gameObject.name))
            allCollectiblesInCurrStage.Add(AC);
    }
}
