using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SubDivAll))]
 public class SubDivAllEditor : Editor {
     public override void OnInspectorGUI () {
         DrawDefaultInspector();
         //This draws the default screen.  You don't need this if you want
         //to start from scratch, but I use this when I'm just adding a button or
         //some small addition and don't feel like recreating the whole inspector.
         if(GUILayout.Button("Bake")) {
             //add everthing the button would do.
             SubDivAll s = (SubDivAll)target;
             s.Bake();
         }
    }
 }
