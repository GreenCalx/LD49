using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble.AI;

public class WkzNPC : SchAIAgent
{
    
    public enum BEHAVIOUR { HOSTILE, NEUTRAL, FRIENDLY };
    
    [Header("WONKERZ NPC\nMAND")]
    public BEHAVIOUR behaviour;
    public string name;
    public CameraFocusable cameraFocusable;

}
