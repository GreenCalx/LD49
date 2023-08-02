using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIBindingElement : UISelectableElement
{
    public GameObject name;
    public GameObject binding;
    public int inputKey;
    public bool isNegativeAxis;

    public void SetBinding(int c, Device d){

        if (d == Device.Keyboard) {
            (Parent as UIBindings).SetBinding(c, d, inputKey, isNegativeAxis);
            if (binding) {
                binding.GetComponent<TMP_Text>().text = ((KeyCode)c).ToString();
            }
        }

        if (d == Device.Joystick0) {
            XboxJoystickCode code = XboxJoystickCode.A;
            if (c < (int)XboxJoystickCode.A || c > (int)XboxJoystickCode.Count){
                code = JoystickEnumUtils.ConvertFromUnityKeyCode((KeyCode)c);
            }
            else
            {
                code = (XboxJoystickCode)c;
            }

            (Parent as UIBindings).SetBinding((int)code, d, inputKey, isNegativeAxis);

            GetComponentInChildren<RawImage>().texture = JoystickEnumUtils.GetButtonImage(code);

            if (binding) {
                binding.GetComponent<TMP_Text>().text = JoystickEnumUtils.GetButtonName(code);
            }
        }
    }

    public void SetAsParent(){
        (Parent as UIBindings).waitingForInput.SetParent(this);
    }
}
