using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buildinfos : MonoBehaviour
{
    // Start is called before the first frame update
    void OnGUI()
    {
	    GUI.Label(new Rect(0, 0, 100, 100), ((int)(1.0f / Time.smoothDeltaTime)).ToString());
        GUI.Label(new Rect(0, 20, 200, 100), "Build version : " + Constants.MajorVersion + "." + Constants.MinorVersion);
        GUI.Label(new Rect(0, 40, 200, 100), "Time since startup : " + Time.realtimeSinceStartup);
    }
}
