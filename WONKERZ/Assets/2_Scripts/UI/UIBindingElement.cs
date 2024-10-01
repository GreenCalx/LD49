using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Schnibble;
using Schnibble.UI;
using Schnibble.Managers;

public class UIBindingElement : UITextTab
{
    public UILabel    bindingLabel;
    public RawImage   bindingImage;

    public PlayerInputs.InputCode inputKey;

    public void SetBinding(Controller.InputCode c){

        (parent as UIBindings).SetBinding(c, inputKey);

        bindingImage.texture = Controller.XboxController.GetCodeDefaultTexture(c);
        bindingLabel.content = Controller.XboxController.GetCodeName(c);
    }

    public override void Activate() {
        var uiBindings =(parent as UIBindings);
        if (uiBindings == null) {
            this.LogError("Parent should be UIBindings but is not.");
            return;
        }
        uiBindings.waitingForInput.parent = this;
        uiBindings.waitingForInput.Show();
    }
}
