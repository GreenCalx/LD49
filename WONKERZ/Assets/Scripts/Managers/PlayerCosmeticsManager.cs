using System;
using UnityEngine;
using System.Collections.Generic;
using Schnibble;

/**
*   Cosmetic element
*/
public class CosmeticElement
{
    public string name = "";
    public COLORIZABLE_CAR_PARTS carPart;

    public string   matName     = "";
    public Mesh     modelName   ;

    public CosmeticElement()
    {
        name = "default";
        carPart = COLORIZABLE_CAR_PARTS.ANY;
        matName = "CarColor";
    }

    public CosmeticElement(string iName, COLORIZABLE_CAR_PARTS iPart, string iMatName)
    {
        name = iName;
        carPart = iPart;
        matName = iMatName;
    }

    public CosmeticElement(string iName, COLORIZABLE_CAR_PARTS iPart, Mesh iModelName)
    {
        name = iName;
        carPart = iPart;
        modelName = iModelName;
    }
}


/**
*   Handles player's Colors, Skins, Decals, etc...
*/
public class PlayerCosmeticsManager : MonoBehaviour
{
        // TODO : Find a way to expose string(skin_name)/Mesh(skin itself) in editor
    [Serializable]
    public struct SkinDesc
    {
        public Mesh mesh;
        public COLORIZABLE_CAR_PARTS carPart;
    };

    [Serializable]
    public struct SkinBundle
    {
        public string key;
        public List<SkinDesc> descriptors;
    };

    public List<SkinBundle> skinname_mesh_dico;
    private Dictionary<string, SkinBundle> local_skin_dico;

    [Header("Internals")]
    public List<CosmeticElement> availableCosmetics = new List<CosmeticElement>();

    private static List<GameObject> players = new List<GameObject>();

    private static PlayerCosmeticsManager inst;

    public static PlayerCosmeticsManager Instance
    {
        get { return inst ?? (inst = Access.PlayerCosmeticsManager()); }
        private set { inst = value; }
    }

    void Start()
    {
        initSkinDictionary();

        resetPlayersToCustomize();
        initCustomizationFromPlayer();
    }

    private void initSkinDictionary()
    {
        local_skin_dico = new Dictionary<string, SkinBundle>();
        foreach(SkinBundle sb in skinname_mesh_dico)
        {
            local_skin_dico.Add( sb.key, sb);
        }
    }

    public void addCosmetic(CosmeticElement iCE)
    {
        if (!availableCosmetics.Exists(e => e.name == iCE.name))
            availableCosmetics.Add(iCE);
    }

    public void addPlayerToCustomize(GameObject iGO)
    {
        if (!!iGO && !players.Contains(iGO))
        {
            players.Add(iGO);
        }
    }

    public void removePlayerToCustomize(GameObject iGO)
    {
        if (!!iGO)
            players.Remove(iGO);
    }

    public void resetPlayersToCustomize()
    {
        players.Clear();
        players.Add(Access.Player().gameObject); 
    }

    private void initCustomizationFromPlayer()
    {
        // COLORS
        addCosmetic(  new CosmeticElement("default_body_primary", COLORIZABLE_CAR_PARTS.MAIN, "CarColor") );
        addCosmetic(  new CosmeticElement("default_body_secondary", COLORIZABLE_CAR_PARTS.FRONT_BUMP, "CarCommonMat") );
        addCosmetic(  new CosmeticElement("default_wheel_primary", COLORIZABLE_CAR_PARTS.WHEELS, "CarCommonMat") );

        // SKINS
        GameObject p = Access.Player().gameObject;

        MeshFilter[] pRends = p.GetComponentsInChildren<MeshFilter>();
        int n_parts = pRends.Length;
        for (int i=0; i < n_parts; i++)
        {
            MeshFilter rend = pRends[i];
            CarColorizable cc_color = rend.gameObject.GetComponent<CarColorizable>();
            if (!!cc_color)
            {
                // TODO : Prevent duplicated at init (multiple car parts sharing same cosmetic)
               // addCosmetic( new CosmeticElement("default", cc_color.part, cc_color.partSkinName) );
            } 
        }

    }

    public CarColorizable getCustomizationOfPart(COLORIZABLE_CAR_PARTS iPart)
    {
        CarColorizable retval = null;
        MeshFilter[] pRends = Access.Player().GetComponentsInChildren<MeshFilter>();
        int n_parts = pRends.Length;
        for (int i=0; i < n_parts; i++)
        {
            MeshFilter rend = pRends[i];
            CarColorizable cc_color = rend.gameObject.GetComponent<CarColorizable>();
            if (!!cc_color && cc_color.part == iPart)
            { 
                retval = cc_color;
                break;
            }
        }
        return retval;
    }

    public void colorize(string iMatName, COLORIZABLE_CAR_PARTS iPart)
    {
        Material mat = Resources.Load(iMatName, typeof(Material)) as Material;

        foreach (GameObject p in players)
        {
            MeshRenderer[] pRends = p.GetComponentsInChildren<MeshRenderer>();
            int n_parts = pRends.Length;
            for (int i=0; i < n_parts; i++)
            {
                MeshRenderer rend = pRends[i];
                CarColorizable cc_color = rend.gameObject.GetComponent<CarColorizable>();
                if (!!cc_color && cc_color.part == iPart)
                { 
                    rend.material = mat; 
                    cc_color.materialName = iMatName;
                }
            
            //if (!!pRend)
            //    pRend.material = newmat;
            }
        }
    }

    public void customize(CarColorizable iCC)
    {
        List<COLORIZABLE_CAR_PARTS> parts = new List<COLORIZABLE_CAR_PARTS>();
        parts.Add(iCC.part);
        customize(iCC.partSkinName, parts);
    }

    public void customize( string skinKey, List<COLORIZABLE_CAR_PARTS> iParts)
    {
        string skinName = skinKey;

        foreach (GameObject p in players)
        {
            MeshFilter[] pRends = p.GetComponentsInChildren<MeshFilter>();
            int n_parts = pRends.Length;
            for (int i=0; i < n_parts; i++)
            {
                MeshFilter rend = pRends[i];
                CarColorizable cc_color = rend.gameObject.GetComponent<CarColorizable>();
                if (!!cc_color && iParts.Contains(cc_color.part))
                { 
                    SkinBundle sb = local_skin_dico[skinName];
                    foreach ( SkinDesc sd in sb.descriptors)
                    {
                        if (cc_color.part == sd.carPart)
                        {
                            rend.sharedMesh = sd.mesh;
                            cc_color.partSkinName = skinName;
                            break;
                        }
                    }
                }
            }
        }        
    }

}