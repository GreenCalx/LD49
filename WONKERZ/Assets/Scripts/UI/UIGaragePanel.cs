using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGaragePanel : MonoBehaviour
{
    private List<GarageUISelectable> selectables;
    
    // Start is called before the first frame update
    void Start()
    {
        selectables = new List<GarageUISelectable>(GetComponentsInChildren<GarageUISelectable>());
        if (selectables.Count < 0)
            Debug.LogWarning("No GarageUISelectable found in UIGaragePanel.");

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void open()
    {

    }

    public void close()
    {

    }
}
