using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Utils
{
    
    public struct Math{
        
    public static (float, bool) ValidateFloat(float f) {
        bool IsWrongValue = float.IsNaN(f) || float.IsInfinity(f) || Mathf.Abs(f) > 10000;
        return (IsWrongValue ? 0 : f, !IsWrongValue);
    }
    /// This function return True if the Vector is good
    /// else it returns false, meaning at least one value was corrected.
    public static (Vector3, bool) ValidateForce(Vector3 F) {
        bool HasWrongNumber = true;
        bool IsGoodValue = false;
        (F.x, IsGoodValue) = ValidateFloat(F.x);
        HasWrongNumber &= IsGoodValue;
        (F.y, IsGoodValue) = ValidateFloat(F.y);
        HasWrongNumber &= IsGoodValue;
        (F.z, IsGoodValue) = ValidateFloat(F.z);
        HasWrongNumber &= IsGoodValue;
        
        return (F, HasWrongNumber);
    }
}
    // TODO : Cache player reference to avoid high cost lookup
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
    
    public static void detachControllable<T>(T toDetach)
    {
        InputManager IM = Access.InputManager();
        if (!!IM)
            IM.Detach(toDetach as IControllable);
        else
            Debug.LogWarning("InputManager is null. Failed to detach.");
    }
    public static void detachUniqueControllable<T>(T toDetach)
    {
        InputManager IM = Access.InputManager();
        if (!!IM)
            IM.UnsetUnique(toDetach as IControllable);
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

    public static int getLayerIndex(string iLayerName)
    {
        return LayerMask.NameToLayer(iLayerName);
    }
        
    public static void exitOnError(string e)
    {
        Debug.LogError(e);
        Application.Quit();
    }
}