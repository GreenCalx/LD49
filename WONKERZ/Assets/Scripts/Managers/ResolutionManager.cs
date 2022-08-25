using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResolutionManager : MonoBehaviour
{
    [Header("Tweaks")]
    public float stdUIMoveStep = 0.005f;
    private Vector2 screenSize;
    private Vector2 lastScreenSize;
    private static ResolutionManager inst;
    public static ResolutionManager Instance
    {
        get { return inst ?? (inst = Access.ResolutionManager()); }
        private set { inst = value; }
    }
    // Start is called before the first frame update
    void Start()
    {
        lastScreenSize = new Vector2(Screen.width, Screen.height);   
        screenSize = lastScreenSize;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        if ( screenSize != lastScreenSize )
        {
            lastScreenSize = screenSize;
        }
    }

    public float getHorMoveStep(float iScreenPercentage)
    {
        return lastScreenSize.x * iScreenPercentage;
    }
    public float getVerMoveStep(float iScreenPercentage)
    {
        return lastScreenSize.y * iScreenPercentage;
    }

    public Vector2 getStdMoveStep()
    {
        return lastScreenSize * stdUIMoveStep;
    }
}
