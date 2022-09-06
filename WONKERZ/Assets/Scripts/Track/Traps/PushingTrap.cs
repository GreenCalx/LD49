using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushingTrap : Trap
{
    [Header("Mandatory")]
    public  KickAction  pushingAction;
    
    public  Renderer    loadingHintRendererCarrier;
    public  Material    loadingHintMatRef; // only used to cp later with retrieved mats

    [Header("Tweaks")]
    public Transform  endPos;
    public float timeBeforeReset = 2f;
    public float trapSpeed = 1f;
    public float smoothTimeOnReset = 0.5f;
    
    [Range(0f, 1f)]
    public float emissiveRangeLowPoint;
    [Range(0f, 1f)]
    public float emissiveRangeHighPoint;

    private Vector3 localInitPos;
    private Vector3 localEndPos;
    private Vector3 dampVelocity;
    private GameObject pushingObject;
    private Color currEmissiveColor;
    private Color baseEmissiveColor;
    private Material selfHintMatRef;

    [HideInInspector]
    public bool isTriggered = false;

    // Start is called before the first frame update
    void Start()
    {
        pushingObject = pushingAction.gameObject;
        
        localInitPos    = transform.position;
        localEndPos     = endPos.position;
        pushingObject.transform.position = localInitPos;

        Material[] mats = loadingHintRendererCarrier.materials;
        string matname_as_newinst = loadingHintMatRef.name + Constants.EXT_INSTANCE;
        for (int i=0; i< mats.Length; i++)
        {    
            if (mats[i].name == matname_as_newinst)
            {
                selfHintMatRef = mats[i];
                baseEmissiveColor =selfHintMatRef.GetColor("_EmissionColor");
                break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isTriggered)
        {
            pushingObject.transform.position = Vector3.MoveTowards(pushingObject.transform.position, localEndPos, trapSpeed * Time.deltaTime);
            if ( Vector3.Distance(pushingObject.transform.position , localEndPos) < 0.1f )
            {
                isTriggered = false;
            }
        
        } else if ( Vector3.Distance(pushingObject.transform.position, localInitPos) > 0.1f ) 
        {
            pushingObject.transform.position = Vector3.SmoothDamp(pushingObject.transform.position, localInitPos, ref dampVelocity, smoothTimeOnReset, trapSpeed);
        }


    }

    private Color getCurrentEmissiveColor(float iPercent, bool reverse)
    {
        Color new_col = baseEmissiveColor;
        new_col *= (reverse ? 1f-iPercent : iPercent );
        return new_col*new_col;
    }

    public override void OnTrigger()
    {
        isTriggered = true;
        pushingAction.kick();
    }

    public override void OnCharge(float iCooldownPercent)
    {
        // mul color emissive for strength
        selfHintMatRef.SetColor("_EmissionColor", getCurrentEmissiveColor(iCooldownPercent, false));
    }

    public override void OnRest(float iCooldownPercent)
    {
        selfHintMatRef.SetColor("_EmissionColor", getCurrentEmissiveColor(iCooldownPercent, true));
        pushingAction.stopKick();
    }
}
