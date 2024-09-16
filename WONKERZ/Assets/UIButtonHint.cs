using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Schnibble;
using Schnibble.UI;
using Schnibble.Managers;

using Wonkerz;

public class UIButtonHint : UIElement
{
    [SerializeField]
    UILabel  label;
    [SerializeField]
    RawImage image;

    public void SetButton(string button, string text) {
        label.text.text = text;
        var controller = Access.PlayerInputsManager().player1.controllers[0].controller;
        // for now only Xbox
        // NOTE toffe: holy shiet this is a bad design gad damn what the fuck wan I thinking lord save us.
        image.texture = Controller.XboxController.GetCodeDefaultTexture(controller.GetButton((int)controller.GetIdx(button)).codePrimary);
    }

}
