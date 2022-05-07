using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Utils
{
    
    // TODO : Cache player reference to avoid high cost lookup
    // TODO : Cache InputManager
    public static GameObject getPlayerRef()
    {
        GameObject playerRef = null;
        GameObject playerRoot = GameObject.Find( Constants.GO_PLAYER );
        if (!!playerRoot)
        {
            playerRef = playerRoot.GetComponentInChildren<CarController>().gameObject;
            if ( playerRef == null )
            {
                exitOnError("Failed to find a reference to the Player in Utils.getPlayer");
            }
        } else {
            exitOnError("Failed to find a reference to the PlayerRoot in Utils.getPlayer.");
        }
        return playerRef;
    }

    public static InputManager GetInputManager()
    {
        GameObject mgr = GameObject.Find(Constants.GO_MANAGERS);
        return !!mgr ? mgr.GetComponent<InputManager>() : null;
    }

    public static void detachControllable<T>(T toDetach)
    {
        InputManager IM = GetInputManager();
        if (!!IM)
            IM.Detach(toDetach as IControllable);
        else
            Debug.LogWarning("InputManager is null. Failed to detach.");
    }

    public static void attachControllable<T>(T toAttach)
    {
        InputManager IM = GetInputManager();
        if (!!IM)
            IM.Attach(toAttach as IControllable);
        else
            Debug.LogWarning("InputManager is null. Failed to detach.");
    }
        
    public static void exitOnError(string e)
    {
        Debug.LogError(e);
        Application.Quit();
    }
}