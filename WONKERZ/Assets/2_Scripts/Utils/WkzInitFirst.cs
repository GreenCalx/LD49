using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schnibble;

namespace Wonkerz
{
    [DefaultExecutionOrder(-2000)]
    public class WkzInitFirst : MonoBehaviour
    {
        void Awake() {
            // Init managers that should be there at first frame. IE: titleScene.
            // NOTE: Do not test for null => we want to see the null pointer exception easily.

            // TODO: make a SchnibbleManager class to derive from to make sure there is no Awake,Start, etc.. and only init()

            // Inputs
            var wonkIM = Access.GetMgr<WonkPlayerInputManager>();
            wonkIM.init();
            wonkIM.Load();
            // Audio
            var am = Access.GetMgr<AudioListenerManager>();
            am.init();
            // Camera
            var cm = Access.GetMgr<CameraManager>();
            cm.init();
            // Gameplay
            var gpmgr = Access.GetMgr<GameProgressSaveManager>();
            gpmgr.init();
            var gs = Access.GetMgr<GameSettings>();
            gs.init();
            var mgr = Access.GetMgr<PlayerCosmeticsManager>();
            mgr.init();
        }
    }
}
