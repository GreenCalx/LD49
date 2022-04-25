using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Utils
{

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

        
    public static void exitOnError(string e)
    {
        Debug.LogError(e);
        // CRASH.EXIT ?
    }
}