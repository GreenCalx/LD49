using UnityEngine;

public class CutSound : MonoBehaviour
{
    public float Timef = 5;
    public float CurrentTime = 0;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        CurrentTime += Time.deltaTime;
        if (CurrentTime >= Timef) GetComponent<AudioSource>().volume *= 0.99f;
    }
}
