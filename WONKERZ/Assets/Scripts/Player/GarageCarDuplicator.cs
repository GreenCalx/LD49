using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarageCarDuplicator : MonoBehaviour
{
    void Start()
    {
        PlayerColorManager.Instance.addPlayerToColorize(gameObject);
    }

    void OnDestroy()
    {
        PlayerColorManager.Instance.removePlayerToColorize(gameObject);
    }
}
