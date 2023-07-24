using UnityEngine;

public class CollectibleNut : AbstractCollectible
{
    public float animTimeStep = 0.1f;
    public bool startUp;

    [Header("States")]
    public bool spawnedFromDamage;

    [Header("Anim")]
    public ParticleSystem onCollectPSRef_Heaven;
    public ParticleSystem onCollectPSRef_Hell;
    [Range(0f, 10f)]
    public float yOscillation;
    [Range(0f, 5f)]
    public float oscillationSpeed;

    public float yRotationSpeed;
    ///
    private Vector3 initPos;
    private Vector3 minPos;
    private Vector3 maxPos;
    private Vector3 initRot;
    private float elapsedTime;
    private bool isGoingUp;
    private float travelTime;
    private float startTime;

    public AudioSource onCollectSound;

    [Header("OnDamageTweaks")]
    public float yExpulsionSlope = 1.5f;
    public float playerExpulsionForceMul = 2f;
    public Vector3 onDamageSpawnOffset = new Vector3(0f, 2f, 0f);
    public float uncollectableTimeAfterDamage = 0.2f;
    public float timeBeforeDisappearing = 3f;
    private float elapsedTimeAfterDamage = 0f;
    public float blinkFreqAfterDamage = 10f;

    [Range(0f, 10f)]
    public float blinkFreqAddFactor = 1.2f;

    private bool collected = false;

    void Awake()
    {
        elapsedTimeAfterDamage = 0f;
        collected = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (!spawnedFromDamage)
        {
            initRot = transform.eulerAngles;
            initRot = transform.eulerAngles;
            minPos = transform.position - new Vector3(0f, yOscillation, 0f);
            maxPos = transform.position + new Vector3(0f, yOscillation, 0f);
            elapsedTime = 0f;
            isGoingUp = !startUp;
            travelTime = Vector3.Distance(minPos, maxPos);
            startTime = Time.time;
            ///
            transform.position = (startUp) ? maxPos : minPos;

            Access.CollectiblesManager().subscribe(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (spawnedFromDamage)
        {
            elapsedTimeAfterDamage += Time.deltaTime;
            if (elapsedTimeAfterDamage >= timeBeforeDisappearing)
            { Destroy(gameObject); }
            fromDamageAnimate();
            return;
        }

        animate();
    }

    public void setSpawnedFromDamage(Vector3 playerPos)
    {
        elapsedTimeAfterDamage = 0f;

        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;

        float theta = Random.Range(0, 360f);

        CollectiblesManager cm = Access.CollectiblesManager();
        float x_pos = cm.nutSpreadDistanceOnDamage * Mathf.Cos(theta);
        float z_pos = cm.nutSpreadDistanceOnDamage * Mathf.Sin(theta);

        transform.position = playerPos;
        transform.position += onDamageSpawnOffset;
        Vector3 fDir = playerPos.normalized + new Vector3(x_pos, yExpulsionSlope, z_pos);
        rb.AddForce(fDir * playerExpulsionForceMul, ForceMode.Impulse);
        Debug.DrawRay(playerPos, fDir * playerExpulsionForceMul, Color.green, 3, false);
        //transform.position += new Vector3( x_pos, 0.5f, z_pos);

    }

    void animate()
    {
        if ((yOscillation == 0) || (oscillationSpeed == 0))
            return;

        transform.Rotate(new Vector3(0, yRotationSpeed, 0), Space.World);
        ///
        float distCovered = (Time.time - startTime) * oscillationSpeed;
        Vector3 nextPos = (isGoingUp) ? Vector3.Lerp(minPos, maxPos, distCovered / travelTime) :
                                       Vector3.Lerp(maxPos, minPos, distCovered / travelTime);
        if (isGoingUp && (nextPos.y > (maxPos.y - (distCovered / travelTime) / 100)))
        { isGoingUp = !isGoingUp; startTime = Time.time; }
        else if (!isGoingUp && (nextPos.y < (minPos.y + (distCovered / travelTime) / 100)))
        { isGoingUp = !isGoingUp; startTime = Time.time; }

        transform.position = nextPos;
        elapsedTime = 0f;
    }

    void fromDamageAnimate()
    {
        // alpha = (1+ cos(ft))/2, where f increases over time
        MeshRenderer mr = GetComponent<MeshRenderer>();
        Material m = mr.material;
        float alpha_val = (1 + Mathf.Cos(blinkFreqAfterDamage * elapsedTimeAfterDamage)) / 2;

        Color newCol = m.GetColor("_Color");
        newCol.a = alpha_val;
        m.SetColor("_Color", newCol);

        blinkFreqAfterDamage += blinkFreqAddFactor;
    }

    void OnCollisionEnter(Collision iCol)
    {
        if (iCol.gameObject.GetComponent<Ground>() != null)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            //rb.isKinematic = true;
            rb.velocity = Vector3.zero;
        }
    }

    protected override void OnCollect()
    {
        if (collected)
            return;
        collected = true;

        if (spawnedFromDamage)
        {
            if (uncollectableTimeAfterDamage >= elapsedTimeAfterDamage)
                return;
        }

        Schnibble.Utils.SpawnAudioSource(onCollectSound, transform);
        // Effect on collect
        if (Access.CollectiblesManager().collectMod == CollectiblesManager.COLLECT_MOD.HELL)
        {
            if (!!onCollectPSRef_Hell)
            {
                ParticleSystem collectPS = Instantiate(onCollectPSRef_Hell) as ParticleSystem;
                collectPS.transform.position = transform.position;
                collectPS.Play();
                Destroy(collectPS.gameObject, collectPS.main.duration);
            }
        }
        else
        {
            if (!!onCollectPSRef_Heaven)
            {
                ParticleSystem collectPS = Instantiate(onCollectPSRef_Heaven) as ParticleSystem;
                collectPS.transform.position = transform.position;
                collectPS.Play();
                Destroy(collectPS.gameObject, collectPS.main.duration);
            }
        }

        //gameObject.SetActive(false);
        Access.CollectiblesManager().applyCollectEffect(this);
        Destroy(gameObject);
    }
}
