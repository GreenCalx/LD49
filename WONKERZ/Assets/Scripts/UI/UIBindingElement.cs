using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIBindingElement : UISelectableElement
{
    public GameObject name;
    public GameObject binding;
    public string inputKey;
    public bool isNegativeAxis;

    public void SetBinding(int c, Device d){
        (Parent as UIBindings).SetBinding(c, d, inputKey, isNegativeAxis);

        if (d == Device.Keyboard) {
            (Parent as UIBindings).SetBinding(c, d, inputKey, isNegativeAxis);
            if (binding) {
                binding.GetComponent<TMP_Text>().text = ((KeyCode)c).ToString();
            }
        }
        if (d == Device.Joystick0) {
                if (c < (int)XboxJoystickCode.A) {
                    // convert to right enum number, the code was sent by KeyCode and not XboxJoystickCode
                    c = (int)JoystickEnumUtils.ConvertFromUnityKeyCode((KeyCode)c);
                }
                (Parent as UIBindings).SetBinding(c, d, inputKey, isNegativeAxis);
                if (binding) {
                     binding.GetComponent<TMP_Text>().text = ((XboxJoystickCode)c).ToString();
                }      
        }
    }

    public void SetAsParent(){
        (Parent as UIBindings).waitingForInput.SetParent(this);
    }
}
