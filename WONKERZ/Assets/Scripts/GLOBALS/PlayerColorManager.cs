using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// SINGLETON
//// ugly ofc.. tinker a better solution later on when this feature is really useful.
public class PlayerColorManager : MonoBehaviour
{
    private static readonly Dictionary<Color,string>  col_matname_dico = new Dictionary<Color, string>
    { 
         { new Color(0.401f, 1.000f, 0.965f, 1.000f), "CarColor"        },
         { new Color(0.953f, 0.919f, 0.625f, 1.000f), "CarColorYellow"  },
         { new Color(0.953f, 0.624f, 0.943f, 1.000f), "CarColorPink"    }
    };

    // List of all player to retrieve for mat update
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
        get { return inst ?? (inst = GameObject.Find(Constants.GO_MANAGERS).GetComponent<PlayerColorManager>()); }
        private set { inst = value; }
    }

    void Awake()
    {
        playerRefs.Clear();
        foreach( string pname in playerNames)
        {
            GameObject p = GameObject.Find(pname);
            if (!!p)
                playerRefs.Add( p );
        }

        // to keep it inbetween scenes if we store values etc..
        DontDestroyOnLoad(gameObject);
    }

    public void colorize(Color iColor)
    {
        var e = col_matname_dico.GetEnumerator();
        while(e.MoveNext())
        {
            Color c = e.Current.Key;
            if ( ColorsAreEqual(c, iColor) )
            {
                string matname = e.Current.Value;
                if (matname.Length==0)
                    matname = "CarColor"; // default

                string matPath = "Materials/"+matname;
                Material newmat = Resources.Load(matPath, typeof(Material)) as Material;
                foreach( GameObject p in playerRefs )
                {
                    Renderer pRend = p.GetComponentInChildren<Renderer>();
                    if (!!pRend)
                        pRend.material = newmat;
                }
                break;
            }
        }
    }

    public static bool ColorsAreEqual( Color first, Color second)
    {
        bool r = ((float)Mathf.Round(first.r * 100f) / 100f) == ((float)Mathf.Round(second.r * 100f) / 100f);
        bool g = ((float)Mathf.Round(first.g * 100f) / 100f) == ((float)Mathf.Round(second.g * 100f) / 100f);
        bool b = ((float)Mathf.Round(first.b * 100f) / 100f) == ((float)Mathf.Round(second.b * 100f) / 100f);
        bool a = ((float)Mathf.Round(first.a * 100f) / 100f) == ((float)Mathf.Round(second.a * 100f) / 100f);
        return r && g && b && a;
    }
}
