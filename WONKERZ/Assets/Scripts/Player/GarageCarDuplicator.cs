using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarageCarDuplicator : MonoBehaviour
{
    void Start()
    {
        PlayerColorManager.Instance.addPlayerToColorize(gameObject);
        PlayerSkinManager.Instance.addPlayerToCustomize(gameObject);
    }

    void OnDestroy()
    {
        PlayerColorManager.Instance.removePlayerToColorize(gameObject);
        PlayerSkinManager.Instance.removePlayerToCustomize(gameObject);
    }
}
