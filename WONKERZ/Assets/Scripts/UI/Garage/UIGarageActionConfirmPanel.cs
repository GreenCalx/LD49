using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIGarageActionConfirmPanel : UIGarageCancelablePanel
{

    public string action_name;
    public TextMeshProUGUI actionTextField;
    // Delegation
    private Action onConfirm = null;
    public void setConfirmAction(Action methodOnConfirm)
    {
        onConfirm = methodOnConfirm;
    }

    public void setTextBoxField(string iStr)
    {
        actionTextField.text = iStr;
    }

    public override void deactivate()
    {
        base.deactivate();
        onConfirm.Invoke();
        gameObject.SetActive(isActivated);
    }
}
