using UnityEngine;

public class AddDebugShowCulling : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var gos = GameObject.FindObjectsOfType<MeshRenderer>();
        foreach (var go in gos)
        {
            go.gameObject.AddComponent<DebugShowCulling>();
        }
    }
}
