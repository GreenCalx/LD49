using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrickUI : MonoBehaviour
{
    public TMPro.TextMeshProUGUI Text;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void displayTrick( string iTrick )
    {
        Text.SetText(iTrick);
    }
}
