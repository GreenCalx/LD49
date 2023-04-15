using System.Collections.Generic;
using Schnibble;
using UnityEngine;

/// SINGLETON
//// ugly ofc.. tinker a better solution later on when this feature is really useful.
public class PlayerColorManager : MonoBehaviour
{
    private static readonly Dictionary<Color32, string> col_matname_dico = new Dictionary<Color32, string>
    {
         { new Color32(0x66, 0xFF, 0xF6, 0XFF), "CarColor"        },
         { new Color32(0xF3, 0xEA, 0x9F, 0XFF), "CarColorYellow"  },
         { new Color32(0xF3, 0x9F, 0xF0, 0XFF), "CarColorPink"    }
    };

    public static Color currentColor;

    // List of all player to retrieve for mat update : OBSOLETE?
    private static readonly List<string> playerNames = new List<string>
    {
        "Player",
        "PlayerHub",
        "GARAGEUI_CAR"
    };

    private static List<GameObject> playerRefs = new List<GameObject>();

    private static PlayerColorManager inst;

    public static PlayerColorManager Instance
    {
        get { return inst ?? (inst = Access.PlayerColorManager()); }
        private set { inst = value; }
    }

    void Awake()
    {
        playerRefs.Clear();
        foreach (string pname in playerNames)
        {
            GameObject p = GameObject.Find(pname);
            if (!!p)
                playerRefs.Add(p);
        }
        initCurrentColor();
    }

    private void initCurrentColor()
    {
        if (playerRefs.Count <= 0)
        { this.LogError("No player refs in PlayerColorManager to init current color."); return; }

        GameObject p = playerRefs[0];
        Renderer pRend = p.GetComponentInChildren<Renderer>();
        if (pRend == null)
        { this.LogError("No Renderer Component found on player in PlayerColorManager to init current color."); return; }



        foreach (KeyValuePair<Color32, string> kvp in col_matname_dico)
        {
            // material is copied on player, thus unity adds (Instance) extensions to its name
            string matInstName = kvp.Value + Constants.EXT_INSTANCE;
            if (matInstName == pRend.material.name)
            {
                currentColor = kvp.Key;
                this.Log("Current color : " + kvp.Value);
                break;
            }
        }
    }

    public void colorize(Color32 iColor, COLORIZABLE_CAR_PARTS iPart)
    {
        var e = col_matname_dico.GetEnumerator();
        while (e.MoveNext())
        {
            Color32 c = e.Current.Key;
            if (ColorsAreEqual(c, iColor))
            {
                string matname = e.Current.Value;
                if (matname.Length == 0)
                    matname = "CarColor"; // default

                string matPath = matname;
                Material newmat = Resources.Load(matPath, typeof(Material)) as Material;
                foreach (GameObject p in playerRefs)
                {
                    MeshRenderer[] pRends = p.GetComponentsInChildren<MeshRenderer>();
                    int n_parts = pRends.Length;
                    for (int i=0; i < n_parts; i++)
                    {
                        MeshRenderer rend = pRends[i];
                        CarColorizable cc_color = rend.gameObject.GetComponent<CarColorizable>();
                        if (!!cc_color && cc_color.part == iPart)
                        { rend.material = newmat; }
                    }

                    //if (!!pRend)
                    //    pRend.material = newmat;
                }
                currentColor = c;
                break;
            }
        }
    }

    public static bool ColorsAreEqual(Color32 first, Color32 second)
    {
        bool r = ((float)Mathf.Round(first.r * 100f) / 100f) == ((float)Mathf.Round(second.r * 100f) / 100f);
        bool g = ((float)Mathf.Round(first.g * 100f) / 100f) == ((float)Mathf.Round(second.g * 100f) / 100f);
        bool b = ((float)Mathf.Round(first.b * 100f) / 100f) == ((float)Mathf.Round(second.b * 100f) / 100f);
        bool a = ((float)Mathf.Round(first.a * 100f) / 100f) == ((float)Mathf.Round(second.a * 100f) / 100f);
        return r && g && b && a;
    }

    public Color getCurrentColor()
    {
        return currentColor;
    }
}
