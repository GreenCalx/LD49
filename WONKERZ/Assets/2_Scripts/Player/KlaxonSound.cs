using UnityEngine;

public class KlaxonSound : MonoBehaviour
{
    public float WaitTime = 2;
    private float CurrentTime = 0;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        CurrentTime += Time.deltaTime;
        if (CurrentTime > WaitTime) GetComponent<AudioSource>().enabled = true;

    }
}
