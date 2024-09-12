using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class buildinfos : MonoBehaviour
{
    public static readonly string version = "Build version : " + Constants.MajorVersion + "." + Constants.MinorVersion;
    // Start is called before the first frame update
    void OnGUI()
    {
	    GUI.Label(new Rect(0, 0, 100, 100), ((int)(1.0f / Time.unscaledDeltaTime)).ToString());
        GUI.Label(new Rect(0, 20, 200, 100), version);
        GUI.Label(new Rect(0, 40, 200, 100), "Time since startup : " + Time.realtimeSinceStartup);
    }
}
