using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
        }
    }

    public void Activate( Vector3 iSteer = default(Vector3) )
    {
        foreach (var rb in objects)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.AddExplosionForce(force, transform.position, radius, upmodif);
            rb.AddForce(iSteer/3, ForceMode.Acceleration);
        }

        GetComponent<Rigidbody>().AddExplosionForce(force, transform.position, radius, upmodif, ForceMode.Acceleration);

        Time.timeScale = 0.5f;
        isStarted = true;
        scalingTimerCurrent = 0f;

        if (deathScreen==null)
            deathScreen = Instantiate(deathUIRef);
        deathScreen.SetActive(true);
        deathText = deathScreen.GetComponent<DeathUI>().deathText;
        Destroy( deathScreen, Access.CameraManager().deathCamDuration);
        
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

            var c =  deathText.color;
            c.a = Mathf.SmoothStep(timeScale, 1f, Mathf.Clamp01(scalingTimerCurrent / scalingTimer));
            deathText.color = c;
        }
    }
}
