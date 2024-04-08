using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WWPalaceMaze : MonoBehaviour
{
    public enum COL_SHAPES { NONE, TRI, SQR, STAR, ROUND};
    [Header("Refs")]
    public bool autoFillDoors = true;
    public Transform doorsHandleForAutofill;
    public List<WWPalaceMazeDoor> mazeDoors;
    public float doorNotifCD = 1f;

    [Header("INTERNALS")]
    private float lastNotifElapsed;

    

    public void init()
    {
        if (autoFillDoors)
        {
            mazeDoors.Clear();
            foreach(Transform child in doorsHandleForAutofill)
            {
                WWPalaceMazeDoor door = child.GetComponent<WWPalaceMazeDoor>();
                if (null!=door)
                {
                    mazeDoors.Add(door);
                }
            }
        }
    }

    public void notifyEnterNewRoom(WWPalaceMazeDoor iDoor)
    {
        if (lastNotifElapsed < doorNotifCD)
        {
            Debug.Log("maze notified : CD denied");
            return;
        }

        if (iDoor.IsSameNumber())
        {
            lockSameNumbers(true);
            lockSameShapes(false);
            Debug.Log("maze notified : lock same shapes");
            
        }
        if (iDoor.IsSameShape())
        {
            lockSameShapes(true);
            lockSameNumbers(false);
            Debug.Log("maze notified : lock same numbers");
        }

        lastNotifElapsed = 0f;
    }


    // Start is called before the first frame update
    void Start()
    {
        init();
    }

    void Update()
    {
        if (lastNotifElapsed < doorNotifCD)
        {
            lastNotifElapsed += Time.deltaTime;
        }
    }


    public void lockSameNumbers(bool iLockState)
    {
        foreach(WWPalaceMazeDoor door in mazeDoors)
        {
            if (door.IsSameNumber())
            {
                if (iLockState)
                    door.CloseDoor();
                else
                    door.OpenDoor();
            }
        }
    }

    public void lockSameShapes(bool iLockState)
    {
        foreach(WWPalaceMazeDoor door in mazeDoors)
        {
            if (door.IsSameShape())
            {
                if (iLockState)
                    door.CloseDoor();
                else
                    door.OpenDoor();
            }
        }
    }
}
