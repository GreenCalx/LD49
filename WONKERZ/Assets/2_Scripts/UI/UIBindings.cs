using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using Schnibble;
using Schnibble.UI;
using Schnibble.Managers;

public class UIBindings : UIPanelTabbedScrollable
{
    public VerticalLayoutGroup layout;
    public GameObject childPrefab;
    public GameObject titlePrefab;

    public UICheckbox toggle;

    public UIWaitInputsPanel waitingForInput;

    public void GetCameraMappingXValue(UICheckbox.UICheckboxValue v){
        v.value = InputSettings.InverseCameraMappingX;
    }

    public void SetCameraMappingXValue(bool v){
        InputSettings.InverseCameraMappingX = v;
    }


    public void SetCameraMappingYValue(bool v){
        InputSettings.InverseCameraMappingY = v;
    }

    public void GetCameraMappingYValue(UICheckbox.UICheckboxValue v){
        v.value = InputSettings.InverseCameraMappingY;
    }


    private void AddTitle(string title)
    {
        var titleGO = Instantiate(titlePrefab, layout.transform);
        titleGO.GetComponentInChildren<TMP_Text>().text = title;
    }

    private void AddInput(PlayerInputs.InputCode code)
    {
        GameInput input = inputMgr.controllers[0].controller.Get((int)code);


        var child = Instantiate(childPrefab, layout.transform);
        var comp = child.GetComponent<UIBindingElement>();

        comp.inputKey = code;

        comp.name.GetComponent<TMP_Text>().text = input.description;

        var inputButton = input as GameInputButton;
        if (inputButton != null)
        {
            comp.binding.GetComponent<TMP_Text>().text = Controller.XboxController.GetCodeName(inputButton.codePrimary);
            comp.GetComponentInChildren<RawImage>().texture = Controller.XboxController.GetCodeDefaultTexture(inputButton.codePrimary);
        }

        var inputAxis = input as GameInputAxis;
        if (inputAxis != null)
        {
            comp.binding.GetComponent<TMP_Text>().text = Controller.XboxController.GetCodeName(inputAxis.inputPrimary.positive);
            comp.GetComponentInChildren<RawImage>().texture = Controller.XboxController.GetCodeDefaultTexture(inputAxis.inputPrimary.positive);
        }

        comp.Parent = this;

        var tab = comp.name.GetComponent<UITab>();
        tab.Parent = this;
        tab.init();

        tab.toActivate = waitingForInput;

        this.tabs.Add(tab);
    }

    public override void activate()
    {
        base.activate();

        Create();
    }

    public override void deactivate()
    {
        base.deactivate();

        CleanUp();
    }

    public void Create() {

        // fuck trying to "dynamically" create the ux
        // create it by hand.
        // this means any new input has to be added here.

        AddTitle("Camera");

        var go = Instantiate(toggle, layout.transform);
        go.onValueChange.AddListener(SetCameraMappingXValue);
        go.getValueFunc.AddListener(GetCameraMappingXValue);
        go.gameObject.SetActive(true);
        this.tabs.Add(go);

        go = Instantiate(toggle, layout.transform);
        go.onValueChange.AddListener(SetCameraMappingYValue);
        go.getValueFunc.AddListener(GetCameraMappingYValue);
        go.gameObject.SetActive(true);
        this.tabs.Add(go);

        AddInput(PlayerInputs.InputCode.CameraX);
        AddInput(PlayerInputs.InputCode.CameraY);
        AddInput(PlayerInputs.InputCode.CameraChange);
        AddInput(PlayerInputs.InputCode.CameraReset);

        AddTitle("General");

        AddInput(PlayerInputs.InputCode.SaveStatesPlant);
        AddInput(PlayerInputs.InputCode.SaveStatesReturn);
        AddInput(PlayerInputs.InputCode.GiveCoinsForTurbo);

        AddTitle("Car");

        AddInput(PlayerInputs.InputCode.Accelerator);
        AddInput(PlayerInputs.InputCode.Break);
        AddInput(PlayerInputs.InputCode.Handbrake);
        AddInput(PlayerInputs.InputCode.Turn);
        AddInput(PlayerInputs.InputCode.Jump);
        AddInput(PlayerInputs.InputCode.Turbo);


        AddInput(PlayerInputs.InputCode.WeightX);
        AddInput(PlayerInputs.InputCode.WeightY);

        AddTitle("UI");

        AddInput(PlayerInputs.InputCode.UIUp);
        AddInput(PlayerInputs.InputCode.UIDown);
        AddInput(PlayerInputs.InputCode.UILeft);
        AddInput(PlayerInputs.InputCode.UIRight);
        AddInput(PlayerInputs.InputCode.UICancel);
        AddInput(PlayerInputs.InputCode.UIValidate);
        AddInput(PlayerInputs.InputCode.UIStart);

    }

    public void CleanUp() {
        for(int i=0; i<layout.transform.childCount; ++i){
            var go =layout.transform.GetChild(i).gameObject;
            Destroy(go);
        }
        this.tabs.Clear();
    }

    public void SetBinding(Controller.InputCode code, PlayerInputs.InputCode key)
    {

        var currentData = inputMgr.controllers[0].controller.Get((int)key);

        var inputButton = currentData as GameInputButton;
        if (inputButton != null)
        {
            // can only be mapped to buttons
            inputMgr.controllers[0].controller.ChangeInput((int)key, new GameInputButton(inputButton.name, inputButton.description, code, inputButton.codeSecondary));
        }

        var inputAxis = currentData as GameInputAxis;
        if (inputAxis != null)
        {
            // can only be mapped to axis
            inputMgr.controllers[0].controller.ChangeInput((int)key, new GameInputAxis(inputAxis.name, inputAxis.description,
                new GameInputAxis.Axis(code, inputAxis.inputPrimary.negative),
                inputAxis.inputSecondary, inputAxis.settings));
        }
    }
}
