using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CinematicCarIntoLifeForce : MonoBehaviour
{
    private UnityEvent callbackOnCloneRdy;
    public GameObject lifeForce_Ref;
    private GameObject lifeForce_Inst;

    [Header("Tweaks")]
    public float explosionForce = 10f;
    public float explosionRadius = 5f;
    public float yLift = 0f;

    ///---
    private Transform prevCamFocus;

    /**
    *   Replaces player for a given amount of time
    */
    public void SpawnPlayerAsLifeForce()
    {
        callbackOnCloneRdy = new UnityEvent();
        callbackOnCloneRdy.AddListener(SpawnLifeForce);
        StartCoroutine(PlayerCloneFactory.GetInstance().SpawnPhysxClone(transform, callbackOnCloneRdy));
    }

    private void SpawnLifeForce()
    {
        // spawn lifeforce
        lifeForce_Inst = Instantiate(lifeForce_Ref);
        lifeForce_Inst.transform.position = Access.Player().transform.position;

        // explode rigidbodies
        List<Rigidbody> bodies = new List<Rigidbody>(GetComponentsInChildren<Rigidbody>());
        foreach( Rigidbody rb in bodies)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.AddExplosionForce(explosionForce, lifeForce_Inst.transform.position, explosionRadius, yLift);
        }

        // Make Player Invisible
        Access.Player().Freeze();
        Access.Player().gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        Stop();
    }

    public void Stop()
    {
        if (lifeForce_Inst!=null)
            Destroy(lifeForce_Inst);
        Access.Player()?.gameObject.SetActive(true);
        Access.Player()?.UnFreeze();
    }

    public void SetAsCamFocus()
    {
        try {
            FollowObjectCamera foc = (FollowObjectCamera)Access.CameraManager().active_camera;
            if (!!foc)
            {
                prevCamFocus = foc.focus;
                foc.focus = lifeForce_Inst.transform;
            }
        } catch (InvalidCastException ice)
        {
            Debug.LogError("CinematicCarIntoLifeForce::SetAsCamFocus needs a FollowObjectCamera as active camera");
            return;
        }
    }

    public void DismissFromCam()
    {
        try {
            FollowObjectCamera foc = (FollowObjectCamera) Access.CameraManager().active_camera;
            if (!!foc)
            {
                if (prevCamFocus!=null)
                    foc.focus = prevCamFocus;
                else
                    foc.focus = Access.Player().transform;
            }
        } catch (InvalidCastException ice)
        {
            Debug.LogError("CinematicCarIntoLifeForce::SetAsCamFocus needs a FollowObjectCamera as active camera");
            return;
        }
    }

}