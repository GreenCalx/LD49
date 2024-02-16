using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Schnibble;




/**
*   Handles player's Colors, Skins, Decals, etc...
*/
public class PlayerCosmeticsManager : MonoBehaviour
{
    public CosmeticCollection defaultCarCollection;
    public CosmeticCollection cosmeticCollection;
    
    // [Serializable]
    // public struct SkinDesc
    // {
    //     public string skinName;

    //     public Mesh meshRef;
    //     public Material matRef;
    //     public GameObject decalGORef;
    // };

    // [Serializable]
    // public struct SkinBundle
    // {
    //     public COLORIZABLE_CAR_PARTS targetCarPart;
    //     public SkinDesc skinDescriptor;
    //     //public string key;
    //     //public List<SkinDesc> descriptors;
    // };

    // public Dictionary<COLORIZABLE_CAR_PARTS, List<SkinDesc>> skinBundles;
    //private Dictionary<string, SkinBundle> local_skin_dico;

    [Header("Internals")]
    public List<int> availableCosmetics = new List<int>();

    private static List<GameObject> players = new List<GameObject>();


    void Start()
    {
        //initSkinDictionary();

        resetPlayersToCustomize();
        initCustomizationFromPlayer();
    }

    // private void initSkinDictionary()
    // {
    //     local_skin_dico = new Dictionary<string, SkinBundle>();
    //     foreach(SkinBundle sb in skinname_mesh_dico)
    //     {
    //         local_skin_dico.Add( sb.key, sb);
    //     }
    // }

    public CosmeticElement getCosmeticFromID(int iID)
    {
        return cosmeticCollection.GetCosmetic(iID);
    }

    public List<CosmeticElement> getDefaultCarParts()
    {
        List<CosmeticElement> retval = new List<CosmeticElement>();
        for(int i=0; i < defaultCarCollection.skins.Length;i++)
        { retval.Add(defaultCarCollection.skins[i]); }
        return retval;
    }

    public void addCosmetic(int iCosmeticElementID)
    {
        if (!availableCosmetics.Contains(iCosmeticElementID))
             availableCosmetics.Add( iCosmeticElementID );
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
        // addCosmetic(  new CosmeticElement("default_color_body_primary", COLORIZABLE_CAR_PARTS.MAIN, "CarColor", "") );
        // addCosmetic(  new CosmeticElement("default_color_body_secondary", COLORIZABLE_CAR_PARTS.FRONT_BUMP, "CarCommonMat", "") );
        // addCosmetic(  new CosmeticElement("default_color_wheel_primary", COLORIZABLE_CAR_PARTS.WHEELS, "CarCommonMat", "") );

        // // SKINS
        // addCosmetic(  new CosmeticElement("default_skin_body_primary", COLORIZABLE_CAR_PARTS.MAIN, "", "default") );
        // addCosmetic(  new CosmeticElement("default_skin_wheel_primary", COLORIZABLE_CAR_PARTS.WHEELS, "", "default") );
        
        // GameObject p = Access.Player().gameObject;

        // MeshFilter[] pRends = p.GetComponentsInChildren<MeshFilter>();
        // int n_parts = pRends.Length;
        // for (int i=0; i < n_parts; i++)
        // {
        //     MeshFilter rend = pRends[i];
        //     CarColorizable cc_color = rend.gameObject.GetComponent<CarColorizable>();
        //     if (!!cc_color)
        //     {
        //         // TODO : Prevent duplicated at init (multiple car parts sharing same cosmetic)
        //        // addCosmetic( new CosmeticElement("default", cc_color.part, cc_color.partSkinName) );
        //     } 
        // }

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

    public void colorize(int iMatSkinID, COLORIZABLE_CAR_PARTS iPart)
    {
        CosmeticElement c_el = getCosmeticFromID(iMatSkinID);
        if (c_el.cosmeticType != CosmeticType.PAINT)
            return;

        colorize(c_el.material, iPart);
    }

    public void colorize(Material iMat, COLORIZABLE_CAR_PARTS iPart)
    {
        //Material mat = Resources.Load(iMatName, typeof(Material)) as Material;

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
                    rend.material = iMat; 
                    //cc_color.materialName = iMatName;
                }
            }
        }
    }

    public void customize( int iSkinID, COLORIZABLE_CAR_PARTS iPart)
    {
        CosmeticElement skin = getCosmeticFromID(iSkinID);
        if (skin.cosmeticType != CosmeticType.MODEL)
            return;

        string skinName = skin.name;
        Mesh mesh       = skin.mesh;

        foreach (GameObject p in players)
        {
            MeshFilter[] pRends = p.GetComponentsInChildren<MeshFilter>();
            int n_parts = pRends.Length;
            for (int i=0; i < n_parts; i++)
            {
                MeshFilter rend = pRends[i];
                CarColorizable cc_color = rend.gameObject.GetComponent<CarColorizable>();
                if (!!cc_color && (iPart == cc_color.part))
                { 
                    rend.sharedMesh = mesh;
                    cc_color.partSkinID = iSkinID;
                    break;
                }
            }
        }        
    }

}