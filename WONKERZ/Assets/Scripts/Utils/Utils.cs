using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            Debug.LogWarning("InputManager is null. Failed to detach.");
    }
    public static void detachUniqueControllable()
    {
        InputManager IM = Access.InputManager();
        if (!!IM)
            IM.UnsetUnique();
        else
            Debug.LogWarning("InputManager is null. Failed to detach unique.");
    }

    public static void attachControllable<T>(T toAttach)
    {
        InputManager IM = Access.InputManager();
        if (!!IM)
            IM.Attach(toAttach as IControllable);
        else
            Debug.LogWarning("InputManager is null. Failed to attach.");
    }

    public static void attachUniqueControllable<T>(T toAttach)
    {
        InputManager IM = Access.InputManager();
        if (!!IM)
            IM.SetUnique(toAttach as IControllable);
        else
            Debug.LogWarning("InputManager is null. Failed to attach unique.");
    }
}