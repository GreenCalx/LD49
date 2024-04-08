using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WkzDoor : MonoBehaviour
{
    [Header("WkzDoor")]
    /// Generic
    public Transform door;
    public bool is_closed = true;
    public Vector3 closedPosition;
    public Vector3 openedPosition;
    public float doorSpeed = 1f;

    public void OpenDoor()
    {
        if (!is_closed)
            return;

        StartCoroutine(openDoor());
    }

    public void CloseDoor()
    {
        if (is_closed)
            return;

        StartCoroutine(closeDoor());
    }

    IEnumerator closeDoor()
    {
        float elapsedTime = 0;
        while (elapsedTime < doorSpeed)
        {
            door.localPosition = Vector3.Lerp(door.position, closedPosition, (elapsedTime / doorSpeed));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        door.localPosition = closedPosition;
        is_closed = true;
        yield return null;
    }

    IEnumerator openDoor()
    {
        float elapsedTime = 0;
        while (elapsedTime < doorSpeed)
        {
            door.localPosition = Vector3.Lerp(door.position, openedPosition, (elapsedTime / doorSpeed));
            elapsedTime += Time.deltaTime;
            yield return null;
        } 
        door.localPosition = openedPosition;
        is_closed = false;
        yield return null;
    }

}
