 // EditorCameraControls.cs
 using UnityEngine;
 using UnityEditor;

 public class EditorCameraControls : EditorWindow
 {
     [MenuItem("Tools/EditorCameraControls")]
     private static void Init()
     {
         GetWindow<EditorCameraControls>();
     }
     int state = -1;
     bool autoDisable = false;
     private void OnEnable()
     {
         bool allOff = true;
         bool allOn = true;
         foreach (var cam in SceneView.GetAllSceneCameras())
         {
             allOff &= !cam.gameObject.activeSelf;
             allOn &= cam.gameObject.activeSelf;
         }
         if (allOn && !allOff)
             state = 1;
         else if (!allOn && allOff)
             state = 0;
         else
             state = -1;
         EditorApplication.playmodeStateChanged += OnPlaymodeChange;
     }
     private void OnDisable()
     {
         EditorApplication.playmodeStateChanged -= OnPlaymodeChange;
     }

     void SetCamState(bool aState)
     {
         foreach (var cam in SceneView.GetAllSceneCameras())
         {
             cam.gameObject.SetActive(aState);
         }
         state = aState?1:0;
         Repaint();
     }

     private void OnGUI()
     {
         if (state == 1)
             GUI.color = Color.green;
         else if(state == 0)
             GUI.color = Color.red;
         else
             GUI.color = Color.yellow;
         GUILayout.BeginHorizontal();
         if (GUILayout.Button("Enable"))
         {
             SetCamState(true);
             SceneView.RepaintAll();
         }
         if (GUILayout.Button("Disable"))
         {
             SetCamState(false);
         }
         GUILayout.EndHorizontal();
         GUI.color = Color.white;
         autoDisable = GUILayout.Toggle(autoDisable,"AutoDisable","Button");

     }
     void OnPlaymodeChange()
     {
         if (autoDisable)
         {
             SetCamState(!EditorApplication.isPlaying);
         }
     }

 }
