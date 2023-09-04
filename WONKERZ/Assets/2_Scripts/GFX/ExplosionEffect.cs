using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    public float explosionDuration = 5f;
    public ParticleSystem[] PS;

    private bool isPlaying = false;
    private float elapsedTime = 0f;
    // Start is called before the first frame update
    void Awake()
    {
        isPlaying = false;
        elapsedTime = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlaying)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= explosionDuration)
            {
                Destroy(gameObject);
            }
        }
    }

    public void runEffect()
    {
        foreach (ParticleSystem ps in PS)
        {
            ps.Play();
        }
        isPlaying = true;
    }
}
