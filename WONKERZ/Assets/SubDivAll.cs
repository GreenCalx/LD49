using UnityEngine;

public class SubDivAll : MonoBehaviour
{
    public void Bake()
    {
        foreach (var g in GameObject.FindObjectsOfType<subdiv>(true))
        {
            g.Bake();
        }
    }
}
