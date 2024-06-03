using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Schnibble;

public class PlayerDebugAttachInputManager : MonoBehaviour
{
    public SchCarController car;

    void Start() {
        var inputMgr = Wonkerz.Access.PlayerInputsManager().player1;
        inputMgr.Attach(car);
    }
}
