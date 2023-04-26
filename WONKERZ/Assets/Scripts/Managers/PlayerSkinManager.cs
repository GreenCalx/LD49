using System;
using System.Collections.Generic;
using Schnibble;
using UnityEngine;

/// SINGLETON
//// also manages car skin
public class PlayerSkinManager : MonoBehaviour
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

    private static List<GameObject> playerRefs = new List<GameObject>();

    private static PlayerSkinManager inst;

    public static PlayerSkinManager Instance
    {
        get { return inst ?? (inst = Access.PlayerSkinManager()); }
        private set { inst = value; }
    }

    void Awake()
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

    public void addPlayerToCustomize(GameObject iGO)
    {
        if (!!iGO && !playerRefs.Contains(iGO))
        {
            playerRefs.Add(iGO);
        }
    }
    public void removePlayerToCustomize(GameObject iGO)
    {
        if (!!iGO)
            playerRefs.Remove(iGO);
    }

    public void resetPlayersToCustomize()
    {
        playerRefs.Clear();
        playerRefs.Add(Access.Player().gameObject); 
    }

    private void initCustomizationFromPlayer()
    {
        if (playerRefs.Count <= 0)
        { this.LogError("No player refs in PlayerColorManager to init current color."); return; }

        GameObject p = Access.Player().gameObject;

        MeshFilter[] pRends = p.GetComponentsInChildren<MeshFilter>();
        int n_parts = pRends.Length;
        for (int i=0; i < n_parts; i++)
        {
            MeshFilter rend = pRends[i];
            CarColorizable cc_color = rend.gameObject.GetComponent<CarColorizable>();
            if (!!cc_color)
            {
                for (int j=1;j<playerRefs.Count;j++)
                {
                    MeshFilter[] otherMFilters = playerRefs[j].GetComponentsInChildren<MeshFilter>();
                    for (int k=0;k<otherMFilters.Length;k++)
                    {
                        MeshFilter otherMFilter = otherMFilters[k];
                        CarColorizable cc_other = otherMFilter.gameObject.GetComponent<CarColorizable>();
                        if (!!cc_other && (cc_other.part==cc_color.part))
                        {
                            otherMFilter.sharedMesh = rend.sharedMesh;
                        }
                    }

                }
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

        foreach (GameObject p in playerRefs)
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
}
