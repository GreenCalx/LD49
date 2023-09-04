using UnityEngine;
using Schnibble;

public class SpikedSphere : Trap
{
    public int damageOnCollide = 2;
    public float lifetime = 60f;

    public bool trigger = false;


    private Rigidbody rb;
    private float elapsed;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        elapsed = 0f;
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed > lifetime)
            Destroy(gameObject);
    }


    void OnCollisionEnter(Collision iCol)
    {
        PlayerController cc = iCol.gameObject.GetComponent<PlayerController>();
        if (!!cc)
        {
            ContactPoint cp = iCol.contacts[0];
            cc.takeDamage(damageOnCollide, cp.point, cp.normal);
        }
    }

    public override void OnTrigger(float iCooldownPercent = 1f)
    {
        rb.isKinematic = false;
    }

    public override void OnRest(float iCooldownPercent = 1f) { }

    public override void OnCharge(float iLoadPercent = 1f) { }
}
