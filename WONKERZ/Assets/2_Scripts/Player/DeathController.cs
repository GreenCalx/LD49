using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Schnibble;

public class DeathController : MonoBehaviour
{
    [Header("Mandatory")]
    public GameObject deathUIRef;

    public List<Rigidbody> objects;
    public float force;
    public float radius;
    public float upmodif;

    public float timeBeforeDeletion = 60f;

    public float timeScale;
    public float scalingTimer;
    private float scalingTimerCurrent = 0;

    public bool isStarted = false;

    public GameObject deathScreen;
    public TMP_Text deathText;
    // Start is called before the first frame update
    void Start()
    {
        if (isStarted)
            return;

        foreach (var rb in objects)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        
            updateMeshFromPlayer(rb.gameObject);
        }
    }

    public void updateMeshFromPlayer(GameObject iGO)
    {
        CarColorizable cc = iGO.GetComponent<CarColorizable>();
        MeshRenderer mr = iGO.GetComponent<MeshRenderer>();
        MeshFilter mf = iGO.GetComponent<MeshFilter>();

        if (!!cc && !!mr && !!mf)
        {
            // Get Player ref  
            GameObject p = Access.Player().gameObject;
            MeshFilter[] pRends = p.GetComponentsInChildren<MeshFilter>();
            int n_parts = pRends.Length;
            for (int i=0; i < n_parts; i++)
            {
                MeshFilter pfilt = pRends[i];
                MeshRenderer prend = pfilt.gameObject.GetComponent<MeshRenderer>();
                CarColorizable player_cc = pfilt.gameObject.GetComponent<CarColorizable>();
                if (!!player_cc && !!prend)
                {
                    if (player_cc.part == cc.part)
                    {
                        mr.material     = prend.material;       // colorize
                        mf.sharedMesh   = pfilt.sharedMesh;     // customize
                        break;
                    }
                }
            } 

        }
    }

    public void Activate(Vector3 iSteer = default(Vector3))
    {
        foreach (var rb in objects)
        {
            updateMeshFromPlayer(rb.gameObject);

            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.AddExplosionForce(force, transform.position, radius, upmodif);
            rb.AddForce(iSteer / 3, ForceMode.Acceleration);
        }

        //GetComponent<Rigidbody>().AddExplosionForce(force, transform.position, radius, upmodif, ForceMode.Acceleration);

        Time.timeScale = 0.5f;
        isStarted = true;
        scalingTimerCurrent = 0f;

        if (deathScreen == null)
            deathScreen = Instantiate(deathUIRef);
        deathScreen.SetActive(true);
        deathText = deathScreen.GetComponent<DeathUI>().deathText;
        Destroy(deathScreen, Access.CameraManager().deathCamDuration);

        Destroy(gameObject, timeBeforeDeletion);
    }

    public void Deactivate()
    {
        foreach (var rb in objects)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        var player = GetComponent<CarController>();
        Time.timeScale = 1f;
        isStarted = false;

        deathScreen.SetActive(false);
    }

    void Update()
    {
        if (isStarted)
        {
            scalingTimerCurrent += Time.unscaledDeltaTime;
            Time.timeScale = Mathf.SmoothStep(timeScale, 1f, Mathf.Clamp01(scalingTimerCurrent / scalingTimer));

            var c = deathText.color;
            c.a = Mathf.SmoothStep(timeScale, 1f, Mathf.Clamp01(scalingTimerCurrent / scalingTimer));
            deathText.color = c;
        }
    }
}
