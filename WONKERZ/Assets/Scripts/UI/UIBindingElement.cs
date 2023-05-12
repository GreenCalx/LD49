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

    public void SetBinding(KeyCode c){
        binding.GetComponent<TMP_Text>().text = c.ToString();
        (Parent as UIBindings).SetBinding(c, inputKey, isNegativeAxis);
    }

    public void SetAsParent(){
        (Parent as UIBindings).waitingForInput.SetParent(this);
    }
}
