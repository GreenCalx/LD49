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

    public CollectibleJar jar;

    // Start is called before the first frame update
    void Start()
    {
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
}
