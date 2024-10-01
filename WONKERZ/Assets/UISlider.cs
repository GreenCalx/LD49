using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using Schnibble.Managers;
using Schnibble.UI;

public class UISlider : UIPanel
{
    public Image   fill;
    public float   value  = 0.0f;
    public Vector2 minMax = new Vector2(0, 1);
    public float   step   = 0.1f;
    public float   stepMax = 1f;

    public enum StepMode {
        None,
        TimeIsStepSize,
        TimeIsStepLatch,
    };
    public StepMode stepMode;
    float currentStepSize;
    float currentStepLatch;
    float stepModeTimer;

    public RectTransform cursor;
    public UILabel cursorHint;
    public string cursorHintFormat;

    public UnityEvent onValueChange;

    void clampValue() {
        value = Mathf.Clamp(value, minMax.x, minMax.y);
    }

    // value between 0 and 1
    public float valueNormalized {
        get => Mathf.Clamp01((value - minMax.x) / (minMax.y - minMax.x));
    }

    override protected void ProcessInputs(InputManager currentMgr, GameController Entry) {

        // While the player wants to move : needed to increment step timer
        // for stepmode behaviour.
        if (Entry.GetButtonState(inputLeft).heldDown
            || Entry.GetButtonState(inputRight).heldDown) {
                stepModeTimer += Time.unscaledDeltaTime;
            } else {
                stepModeTimer = 0;
            }

        switch(stepMode) {
            case StepMode.None: {
                //notthing to do
            } break;

            case StepMode.TimeIsStepSize: {
                currentStepLatch = UISettings.inputLatch;
                currentStepSize  = Mathf.Max(0, step + stepModeTimer * (stepMax - step));
            } break;

            case StepMode.TimeIsStepLatch: {
                currentStepLatch = Mathf.Max(0.03f, UISettings.inputLatch - stepModeTimer * UISettings.inputLatch);
                currentStepSize  = step;
            } break;
        }

        if (inputLatchTimer > 0.0f) inputLatchTimer -= Time.unscaledDeltaTime;
        else {
            if (Entry.GetButtonState(inputLeft).heldDown) {
                value -= currentStepSize;
                clampValue();

                ValueChanged();

                inputLatchTimer = currentStepLatch;
            } else if (Entry.GetButtonState(inputRight).heldDown) {
                value += currentStepSize;
                clampValue();

                ValueChanged();

                inputLatchTimer = currentStepLatch;
            }

            // cancel
            base.ProcessInputs(currentMgr, Entry);
        }
    }

    // TODO: should be private and called when setting value.
    public void ValueChanged() {
        // update fill bar
        fill.fillAmount = valueNormalized;
        // update cursor
        var fillBarRect                = fill.GetComponent<RectTransform>().rect;
        var cursorPosition             = fillBarRect.xMin + fill.fillAmount * (fillBarRect.width);
        var cursorLocPos               = cursor.transform.localPosition;
        cursorLocPos.x                 = cursorPosition;
        cursor.transform.localPosition = cursorLocPos;
        cursorHint.content             = value.ToString(cursorHintFormat);

        onValueChange?.Invoke();
    }

    override public void Activate() {
        StartInputs();
        base.Activate();

        currentStepSize  = step;
        currentStepLatch = UISettings.inputLatch;

        cursor.gameObject.SetActive(true);
    }

    override public void Deactivate() {
        base.Deactivate();
        StopInputs();
        cursor.gameObject.SetActive(false);
    }
}
