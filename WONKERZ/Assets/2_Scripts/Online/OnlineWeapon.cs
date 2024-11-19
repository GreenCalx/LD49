using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;

public class OnlineWeapon : NetworkBehaviour
{
    public GameObject selfAttackModelFx;
    private IEnumerator weaponCo;
    private UnityEvent callbackOnFinish;
    public void Thrust(float iThrustTime, float iRotSpeed, float iZOffset, UnityEvent iCallback)
    {
        weaponCo = ThrustCo(iThrustTime,iRotSpeed, iZOffset,iCallback);
        StartCoroutine(weaponCo);
    }
    IEnumerator ThrustCo(float iThrustTime, float iRotSpeed, float iZOffset, UnityEvent iCallback)
    {
        float elapsedThrustTime = 0f;
        float zRot = transform.localRotation.z;
        if (selfAttackModelFx)
            selfAttackModelFx.SetActive(true);
        while (elapsedThrustTime < iThrustTime)
        {
            transform.localPosition = new Vector3(0f,0f,iZOffset);
            zRot += Time.deltaTime * iRotSpeed;
            transform.localRotation = Quaternion.Euler(0, 0, zRot);
            elapsedThrustTime += Time.deltaTime;
            yield return null;
        }
        if (selfAttackModelFx)
            selfAttackModelFx.SetActive(false);
        transform.localPosition = Vector3.zero;
        iCallback.Invoke();
    }


}
