using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UICheckbox : UITextTab
{
    public UICheckboxValue value = new UICheckboxValue();
    public Image checkmark;

    public class UICheckboxValue {
        public bool value;
    }

    [SerializeField]
    UnityEvent<UICheckboxValue> getValueFunc;
    [SerializeField]
    UnityEvent<bool> onValueChange;

    public override void activate()
    {
        base.activate();
        toggle();
    }

    // hack
    public override void select()
    {
        base.select();
        getValue();
    }

    // hack
    public override void deselect()
    {
        base.deselect();
        getValue();
    }

    private void toggle(){
        value.value = !value.value;
        checkmark.enabled = value.value;
        onValueChange.Invoke(value.value);
    }

    private void getValue() {
        getValueFunc.Invoke(value);
    }
}
