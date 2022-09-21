using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(subdiv))]
 public class subdivEditor : Editor {
     public override void OnInspectorGUI () {
         DrawDefaultInspector();
         //This draws the default screen.  You don't need this if you want
         //to start from scratch, but I use this when I'm just adding a button or
         //some small addition and don't feel like recreating the whole inspector.
         if(GUILayout.Button("Bake")) {
             //add everthing the button would do.
             subdiv s = (subdiv)target;
             s.Bake();
         }
    }
 }
