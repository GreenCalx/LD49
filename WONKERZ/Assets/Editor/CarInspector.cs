using UnityEditor;
using UnityEngine;

#if false
[CustomEditor(typeof(CarController))]
public class CarInspector : Editor
{



    class EditorGraph
    {
        public Rect Bounds;
        public Rect Axis;
        public Vector2 GridSize;
        public EditorGraph(Rect R, Rect Axis, Vector2 GRidSize)
        {
            Bounds = R;
            this.Axis = Axis;
            this.GridSize = GRidSize;
        }

        public void Draw()
        {
            DrawRect(Bounds.xMin, Bounds.yMin, Bounds.xMax, Bounds.yMax, Color.black, Color.white);
            DrawAxis(Color.white);
            DrawGrid(Color.green);
        }
        void DrawAxis(Color C)
        {
            DrawLine(Axis.xMin, 0, Axis.xMax, 0, C);
            DrawLine(0, Axis.yMin, 0, Axis.yMax, C);
        }

        void DrawGrid(Color C)
        {
            float x = Axis.xMin;
            while (x < Axis.xMax)
            {
                x += GridSize.x - (x % GridSize.x);
                if (x != 0)
                    DrawLine(x, Axis.yMin, x, Axis.yMax, C);
            }

            float y = Axis.yMin;
            while (y < Axis.yMax)
            {
                y += GridSize.y - (y % GridSize.y);
                if (y != 0)
                    DrawLine(Axis.xMin, y, Axis.xMax, y, C);
            }
        }
        public Vector3 GraphUnitToDrawUnit(Vector2 V)
        {
            var x = Mathf.Lerp(Bounds.xMin, Bounds.xMax, (V.x - Axis.xMin) / Axis.width);
            var y = Mathf.Lerp(Bounds.yMax, Bounds.yMin, (V.y - Axis.yMin) / Axis.height);
            return new Vector3(x, y, 0);
        }


        void DrawLine(float x, float y, float xx, float yy, Color C)
        {
            var Vertices = new Vector3[2];
            Vertices[0] = GraphUnitToDrawUnit(new Vector2(x, y));
            Vertices[1] = GraphUnitToDrawUnit(new Vector2(xx, yy));

            Handles.color = C;
            Handles.DrawAAPolyLine(2, Vertices);
        }

        public void DrawCurve(Vector3[] Vertices, Color C)
        {
            Handles.color = C;
            Handles.DrawAAPolyLine(2, Vertices);
        }

        void DrawRect(float minX, float minY, float maxX, float maxY, Color BG, Color Outline)
        {
            var Vertices = new Vector3[4];
            Vertices[0] = new Vector3(minX, minY, 0);
            Vertices[1] = new Vector3(maxX, minY, 0);
            Vertices[2] = new Vector3(maxX, maxY, 0);
            Vertices[3] = new Vector3(minX, maxY, 0);
            Handles.DrawSolidRectangleWithOutline(Vertices, BG, Outline);
        }
    }


    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical();
        DrawDefaultInspector();
        EditorGUILayout.EndVertical();

        // trying to plot friction model, motor, etc...
        var GraphHeight = 500f;
        Rect R = EditorGUILayout.BeginVertical(GUILayout.MinHeight(GraphHeight), GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
        R.height = GraphHeight;
        R.width = Screen.width;

        GUILayout.FlexibleSpace();

        var CC = (CarController)target;
        var minY = CC.CarMotor.GetEngineFriction(CC.CarMotor.MaxRPM);
        EditorGraph EG = new EditorGraph(R, new Rect(-1f, -minY, CC.CarMotor.MaxRPM, CC.CarMotor.MaxTorque + 100f), new Vector2(100, 100));
        EG.Draw();

        var Vertices = new Vector3[3];
        Vertices[0] = EG.GraphUnitToDrawUnit(new Vector2(CC.CarMotor.IdleRPM, -CC.CarMotor.GetEngineFriction(CC.CarMotor.IdleRPM)));
        Vertices[1] = EG.GraphUnitToDrawUnit(new Vector2(CC.CarMotor.PeakRPM, -CC.CarMotor.GetEngineFriction(CC.CarMotor.PeakRPM)));
        Vertices[2] = EG.GraphUnitToDrawUnit(new Vector2(CC.CarMotor.MaxRPM, -CC.CarMotor.GetEngineFriction(CC.CarMotor.MaxRPM)));
        EG.DrawCurve(Vertices, Color.red);



        EditorGUILayout.EndVertical();
    }
}
#endif
