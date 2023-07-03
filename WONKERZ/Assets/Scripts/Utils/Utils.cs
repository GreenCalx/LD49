using UnityEngine;
using Schnibble;

public static partial class Utils
{
    public static GameObject getPlayerRef()
    {
        return Access.Player().gameObject;
    }

    public static void detachControllable<T>(T toDetach)
    {
        InputManager IM = Access.InputManager();
        if (!!IM)
            IM.Detach(toDetach as IControllable);
        else
            SchLog.LogWarn("InputManager is null. Failed to detach.");
    }
    public static void detachUniqueControllable<T>(T toDetach)
    {
        InputManager IM = Access.InputManager();
        if (!!IM)
            IM.UnsetUnique(toDetach as IControllable);
        else
            SchLog.LogWarn("InputManager is null. Failed to detach unique.");
    }

    public static void attachControllable<T>(T toAttach)
    {
        InputManager IM = Access.InputManager();
        if (!!IM)
            IM.Attach(toAttach as IControllable);
        else
            SchLog.LogWarn("InputManager is null. Failed to attach.");
    }

    public static void attachUniqueControllable<T>(T toAttach)
    {
        InputManager IM = Access.InputManager();
        if (!!IM)
            IM.SetUnique(toAttach as IControllable);
        else
            SchLog.LogWarn("InputManager is null. Failed to attach unique.");
    }

    public static bool isPlayer(GameObject iGO)
    {
        if (iGO.GetComponent<Dummy>()!=null)
            return true;

        CarController direct_cc = iGO.GetComponent<CarController>();
        if (!!direct_cc)
            return true;
        // can also be wheels
        CarColorizable carpart = iGO.GetComponent<CarColorizable>();
        if (!!carpart)
            return true;

        if (iGO.transform.parent == null)
            return false;

        CarController[] parent_cc = iGO.GetComponentsInParent<CarController>();
        if (parent_cc != null && parent_cc.Length>0)
            return true;

        return false;
    }

    public static bool colliderIsPlayer(Collider iCollider)
    {
        if (!!iCollider.transform.parent)
            return !!iCollider.transform.parent.GetComponent<PlayerController>();
        else
            return false;
    }

    public static bool collisionIsPlayer(Collision iCollision)
    {
        return !!colliderIsPlayer(iCollision.collider);
    }

    public static bool checkAnyKeyPressed(InputManager.InputData Entry, bool iAxis)
    {
        if (iAxis)
        {
            foreach(string axis in Constants.INPUT_AXIS)
            {
                if (Entry.Inputs[axis].AxisValue != 0)
                    return true;
            }
        }
        foreach(string btn in Constants.INPUT_BTNS)
        {
            if (Entry.Inputs[btn].IsDown)
                return true;
        }
        return false;
    }
}
