using System;
using TMPro;

public class UIGarageActionConfirmPanel : UIGaragePanel
{

    public string action_name;
    public TextMeshProUGUI actionTextField;
    // Delegation
    private Action onConfirm = null;
    public void setConfirmAction(Action methodOnConfirm)
    {
        onConfirm = methodOnConfirm;
    }

    public void confirm()
    {
        onConfirm.Invoke();
    }

    public void setTextBoxField(string iStr)
    {
        actionTextField.text = iStr;
    }

    public override void Activate()
    {
        gameObject.SetActive(true);
        base.Activate();
    }

    public override void Deactivate()
    {
        base.Deactivate();
        gameObject.SetActive(false);
    }
}
