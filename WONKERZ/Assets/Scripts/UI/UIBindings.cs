using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class UIBindings : UIPanelTabbed
{
    public VerticalLayoutGroup layout;
    public GameObject childPrefab;
    public UIWaitInputsPanel waitingForInput;

    public void Create() {
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
    }

    public void CleanUp() {
        for(int i=0; i<layout.transform.childCount; ++i){
            var go =layout.transform.GetChild(i).gameObject;
            Destroy(go);
        }
        this.tabs.Clear();
    }

    public void SetBinding(KeyCode code, string key, bool isNegativeAxis=false){
        if (!isNegativeAxis)
            InputSettings.Mapping[key].Positive[1] = (int)code;
        else
            InputSettings.Mapping[key].Negative[1] = (int)code;
    }
}
