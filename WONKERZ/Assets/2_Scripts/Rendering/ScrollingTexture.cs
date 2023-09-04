using UnityEngine;

public class ScrollingTexture : MonoBehaviour
{

    public float scrollX = 0.5f;
    public float scrollY = 0.5f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float offsetX = Time.deltaTime * scrollX;
        float offsetY = Time.deltaTime * scrollY;
        GetComponent<Renderer>().material.mainTextureOffset += new Vector2(offsetX, offsetY);
    }
}
