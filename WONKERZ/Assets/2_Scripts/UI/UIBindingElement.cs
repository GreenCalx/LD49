using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Schnibble;
using Schnibble.UI;
using Schnibble.Managers;

public class UIBindingElement : UISelectableElement
{
    public GameObject name;
    public GameObject binding;

    public PlayerInputs.InputCode inputKey;

    public override void init() {}
    public override void deinit() {}

    public void SetBinding(Controller.InputCode c){

        (Parent as UIBindings).SetBinding(c, inputKey);

        GetComponentInChildren<RawImage>().texture = Controller.XboxController.GetCodeDefaultTexture(c);

        if (binding) {
            binding.GetComponent<TMP_Text>().text = Controller.XboxController.GetCodeName(c);
        }
    }

    public void SetAsParent(){
        (Parent as UIBindings).waitingForInput.SetParent(this);
    }
}
