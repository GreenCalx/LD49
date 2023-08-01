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
    public UIWaitInputsPanel waitingForInput;

    public void Create() {
        var l1 = GameInputsUtils.uiMappingInfos.GetLength(0);
        for (int i = 0; i < l1; ++i) {
            var title = Instantiate(titlePrefab, layout.transform);
            if (i == 0)
            {
                title.GetComponentInChildren<TMP_Text>().text = "Camera";
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

                comp.inputKey = GameInputsUtils.gameInputsInternalNames[input];
                comp.name.GetComponent<TMP_Text>().text = GameInputsUtils.gameInputsDescription[input];

                var joystickCode = GameInputsUtils.IsAxis(input) ? GameInputsUtils.axisMapping[input,0].joytickCode : GameInputsUtils.buttonsMapping[input].joytickCode;
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
                #if false
                // get input manager and create all available bindings
                foreach (var idata in InputSettings.Mapping){
                    if (idata.Value.IsMouse) continue;

                    var child = Instantiate(childPrefab, layout.transform);

                    var comp = child.GetComponent<UIBindingElement>();

                    comp.inputKey = idata.Key;
                    comp.name.GetComponent<TMP_Text>().text = idata.Key + (idata.Value.IsAxis?" +":"");
                    comp.binding.GetComponent<TMP_Text>().text = ((KeyCode)(idata.Value.Positive[1])).ToString();
                    comp.Parent = this;

                    var tab = comp.name.GetComponent<UITab>();
                    tab.Parent = this;
                    tab.init();

                    tab.toActivate = waitingForInput;

                    this.tabs.Add(tab);

                    if (idata.Value.IsAxis){

                        child = Instantiate(childPrefab, layout.transform);
                        comp = child.GetComponent<UIBindingElement>();
                        comp.inputKey = idata.Key;
                        comp.isNegativeAxis = true;
                        comp.name.GetComponent<TMP_Text>().text = idata.Key + " -";
                        comp.binding.GetComponent<TMP_Text>().text = ((KeyCode)(idata.Value.Negative[1])).ToString();
                        comp.Parent = this;

                        tab = comp.name.GetComponent<UITab>();
                        tab.Parent = this;
                        tab.init();

                        tab.toActivate = waitingForInput;

                        this.tabs.Add(tab);
                    }

                }
                #endif
            }

            public void CleanUp() {
                for(int i=0; i<layout.transform.childCount; ++i){
                    var go =layout.transform.GetChild(i).gameObject;
                    Destroy(go);
                }
                this.tabs.Clear();
            }

            public void SetBinding(int code, Device device, string key, bool isNegativeAxis = false)
            {
                #if false
                if (device == Device.Mouse || device == Device.Keyboard)
                {
                    if (!isNegativeAxis)
                    InputSettings.Mapping[key].Positive[1] = code;
                    else
                    InputSettings.Mapping[key].Negative[1] = code;
                }
                else
                {
                    InputSettings.Mapping[key].Positive[0] = code;
                }
                #endif
            }
        }
