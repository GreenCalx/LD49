using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UIBindings : UIPanelTabbedScrollable
{
    public VerticalLayoutGroup layout;
    public GameObject childPrefab;
    public GameObject titlePrefab;

    public UICheckbox inverseCamera;

    public UIWaitInputsPanel waitingForInput;

    public void Create() {
        var l1 = GameInputsUtils.uiMappingInfos.GetLength(0);
        for (int i = 0; i < l1; ++i) {
            var title = Instantiate(titlePrefab, layout.transform);
            // todo : put in gameinputsutils or else
            if (i == 0)
            {
                title.GetComponentInChildren<TMP_Text>().text = "Camera";
                var go = Instantiate(inverseCamera, layout.transform);
                go.gameObject.SetActive(true);
                this.tabs.Add(go);
            }
            if (i == 1)
            {
                title.GetComponentInChildren<TMP_Text>().text = "General";
            }
            if (i == 2)
            {
                title.GetComponentInChildren<TMP_Text>().text = "Car";
            }
            if (i == 3)
            {
                title.GetComponentInChildren<TMP_Text>().text = "UI";
            }

            var l2 = GameInputsUtils.uiMappingInfos.GetLength(1);
            for (int j = 0; j < l2; ++j)
            {
                var input = GameInputsUtils.uiMappingInfos[i, j] -1;
                if (input < 0) break;

                var child = Instantiate(childPrefab, layout.transform);
                var comp = child.GetComponent<UIBindingElement>();

                comp.inputKey = input;
                comp.name.GetComponent<TMP_Text>().text = GameInputsUtils.gameInputsDescription[input];

                var joystickCode = GameInputsUtils.IsAxis(input) ? GameInputsUtils.axisMapping[input][0].joytickCode : GameInputsUtils.buttonsMapping[input].joytickCode;
                comp.binding.GetComponent<TMP_Text>().text = JoystickEnumUtils.GetButtonName(joystickCode);
                comp.GetComponentInChildren<RawImage>().texture = JoystickEnumUtils.GetButtonImage(joystickCode);
                comp.Parent = this;

                var tab = comp.name.GetComponent<UITab>();
                tab.Parent = this;
                tab.init();

                tab.toActivate = waitingForInput;

                this.tabs.Add(tab);
            }
        }
    }

    public void CleanUp() {
        for(int i=0; i<layout.transform.childCount; ++i){
            var go =layout.transform.GetChild(i).gameObject;
            Destroy(go);
        }
        this.tabs.Clear();
    }

    public void SetBinding(int code, Device device, int key, bool isNegativeAxis = false)
    {
        if (key < (int)GameInputsAxis.Count)
        {
            GameInputsUtils.ChangeInput( (GameInputsAxis)key, (XboxJoystickCode)code);
        }
        else
        {
            GameInputsUtils.ChangeInput((GameInputsButtons)key, (XboxJoystickCode)code);
        }
    }
}
