using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Utils
{
    
    public struct Math
    {
        
        public static (float, bool) ValidateFloat(float f) {
            bool IsWrongValue = float.IsNaN(f) || float.IsInfinity(f) || Mathf.Abs(f) > 10000;
            return (IsWrongValue ? 0 : f, !IsWrongValue);
        }
        /// This function return True if the Vector is good
        /// else it returns false, meaning at least one value was corrected.
        public static (Vector3, bool) ValidateForce(Vector3 F) 
        {
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

        // this is a correction of lamguage implementation of mod
        // in our test the official impl is wrong :  0.5 % 0.1 returns 0.1!!!
        public static int Mod(int a, int b) {
            return ( a - ((int) (a/b)) * b );
        }

        public static float Mod(float a, float b) {
            return a - Mathf.FloorToInt(a/b)*b;
        }

        public static double Mod(double a, double b) {
            return ( a - (double)Mathf.FloorToInt( (float) (a/b)) * b );
        }
    }
    
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
