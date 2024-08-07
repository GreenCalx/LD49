using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble.AI;

namespace Wonkerz
{

    public class WkzNPC : SchAIAgent
    {

        public enum BEHAVIOUR { HOSTILE, NEUTRAL, FRIENDLY };

        [Header("# WONKERZ NPC\nMAND")]
        public BEHAVIOUR behaviour;
        public string npc_name;
        public CameraFocusable cameraFocusable;

    }
}
