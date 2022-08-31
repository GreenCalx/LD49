using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIElement : MonoBehaviour
{
    public UIElement Parent;
}

public abstract class UISelectableElement : UIElement
{
    public UnityEvent onSelect;
    public Color selected_color;
    public UnityEvent onDeselect;
    public Color deselected_color;
    public UnityEvent onActivate;
    public Color activated_color;
    public UnityEvent onDeactivate;

    public float elapsed_time;
    public float selector_latch;

    public UISelectableElement activator;

    public bool copyColorFromParent = false;

    protected virtual void Awake() {
        if (copyColorFromParent && Parent) {
            selected_color = (Parent as UISelectableElement).selected_color;
            deselected_color = (Parent as UISelectableElement).deselected_color;
            activated_color = (Parent as UISelectableElement).activated_color;
        }
    }

    virtual public void setColor(Color C) {}
    virtual public void select(){
        setColor(selected_color);
    }

    virtual public void deselect(){
        setColor(deselected_color);
    }

    virtual public void activate(){
        setColor(activated_color);
    }

    virtual public void deactivate(){

    }
}

public class UIPanel : UISelectableElement
{
    public void animateIn()
    {
        var animators = new List<Animator>(GetComponentsInChildren<Animator>());
        if (animators != null)
        {
            foreach (Animator a in animators)
            {
                a.enabled = true;
                a.updateMode = AnimatorUpdateMode.UnscaledTime; // as we pause game by putting deltaTime to 0
                //a.SetTrigger("animatePanel");
                a.Play("Base Layer.GaragePanelIn", -1, 0);
            }
        }
    }

    public void animateOut()
    {
        var animators = new List<Animator>(GetComponentsInChildren<Animator>());
        if (animators != null)
        {
            foreach (Animator a in animators)
            {
                a.enabled = true;
                a.updateMode = AnimatorUpdateMode.UnscaledTime; // as we pause game by putting deltaTime to 0
                //a.SetTrigger("animatePanel");
                a.Play("Base Layer.GaragePanelOut", -1, 0);
            }
        }
    }
}

public class UITabbedPanel : UIPanel, IControllable
{
    public List<UITab> Tabs;
    public int selected;

    public enum Mode { Horizontal, Vertical };
    public Mode CurrentMode;

    void IControllable.ProcessInputs(InputManager.InputData Entry)
    {
        ProcessInputs(Entry);
    }

    virtual protected void ProcessInputs(InputManager.InputData Entry){
        if (elapsed_time > selector_latch)
        {
            float X = 0;
            if (CurrentMode == Mode.Horizontal)
            {
                X = Entry.Inputs[Constants.INPUT_TURN].AxisValue;
            }
            else if (CurrentMode == Mode.Vertical)
            {
                X = Entry.Inputs[Constants.INPUT_UIUPDOWN].AxisValue;
            }
            if (X < -0.2f)
            {
                SelectTab(PreviousTab());
                elapsed_time = 0f;
            }
            else if (X > 0.2f)
            {
                SelectTab(NextTab());
                elapsed_time = 0f;
            }

        }
        elapsed_time += Time.unscaledDeltaTime;
    }

    public int NextTab() { if (Tabs.Count == 0) return 0; return (selected + 1) % Tabs.Count; }
    public int PreviousTab() { if (Tabs.Count == 0) return 0; return (selected - 1 + Tabs.Count) % Tabs.Count; }
    public int CurrentTab() { return selected; }

    public void SelectTab(int index)
    {
        if (Tabs.Count == 0) return;

        GetTab(CurrentTab()).onDeselect?.Invoke();
        selected = index;
        GetTab(CurrentTab()).onSelect?.Invoke();
    }
    public UITab GetTab(int index) { if (Tabs.Count == 0) return null; return Tabs[selected]; }

    // Increment => SelectTab(NextTab());
    // Decrement => SelectTab(PreviousTab())
}


public abstract class Graph : MonoBehaviour
{
    public Rect displayBounds; // display
    public Rect axisBounds; // graph min max axis size
    public Vector2 gridSize; // x and y axis grid size

    public GraphLine curve;
    public Vector2[] points;

    public int yMul = 1;

    [Serializable]
    public struct Axis
    {
        public GraphLine line;
        public TextMeshProUGUI label;
    }

    [Serializable]
    public struct GraphLine
    {
        public Color color;
        public float thickness;
    }

    public Axis x;
    public GraphLine xGrid;
    public Axis y;
    public GraphLine yGrid;

    public abstract void DrawLine(float x, float y, float xx, float yy, Color C, float thickness);

    public void SetCurve(AnimationCurve curve)
    {
        points = new Vector2[curve.length];

        var keys = curve.keys;
        for (int i = 0; i < points.Length; ++i)
        {
            points[i] = new Vector2(keys[i].time, keys[i].value);
        }
    }

    protected void DrawAxis()
    {
        DrawLine(axisBounds.xMin, 0, axisBounds.xMax, 0, x.line.color, x.line.thickness);
        DrawLine(0, axisBounds.yMin, 0, axisBounds.yMax, y.line.color, y.line.thickness);
    }

    protected void DrawGrid()
    {
        float x = axisBounds.xMin;
        while (x < axisBounds.xMax)
        {
            x += gridSize.x - Utils.Math.Mod(x, gridSize.x);
            if (x != 0)
                DrawLine(x, axisBounds.yMin, x, axisBounds.yMax, xGrid.color, xGrid.thickness);
        }

        float y = axisBounds.yMin;
        while (y < axisBounds.yMax)
        {
            y += gridSize.y - Utils.Math.Mod(y, gridSize.y);
            if (y != 0)
                DrawLine(axisBounds.xMin, y, axisBounds.xMax, y, yGrid.color, yGrid.thickness);
        }
    }

    protected virtual void DrawCurve()
    {
        if (points.Length < 2)
        {
            Debug.LogError("Trying to draw curve with less than two points");
            return;
        }

        for (int i = 0; i < points.Length - 1; ++i)
        {
            var p1 = points[i];
            var p2 = points[i + 1];
            DrawLine(p1.x, p1.y, p2.x, p2.y, curve.color, curve.thickness);
        }
    }

    public virtual void Draw()
    {
        DrawAxis();
        DrawGrid();
        DrawCurve();
    }

    public Vector3 GraphUnitToDrawUnit(Vector2 V)
    {
        var x = Mathf.Lerp(displayBounds.xMin, displayBounds.xMax, (V.x - axisBounds.xMin) / axisBounds.width);
        var y = Mathf.Lerp(displayBounds.yMin, displayBounds.yMax, (V.y - axisBounds.yMin) / axisBounds.height);
        return new Vector3(x, y, 0);
    }

    public Vector3 GraphUnitToDrawUnitAbsolute(Vector2 V)
    {
        var unitsX = (displayBounds.xMax - displayBounds.xMin)/(axisBounds.xMax - axisBounds.xMin);
        var unitsY = (displayBounds.yMax - displayBounds.yMin)/(axisBounds.yMax - axisBounds.yMin);
        return new Vector3(unitsX*V.x,unitsY*V.y, 0);
    }


}


public class InspectorGraph : Graph
{
    public override void DrawLine(float x, float y, float xx, float yy, Color C, float thickness)
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

    public override void Draw()
    {
        DrawRect(displayBounds.xMin, displayBounds.yMin, displayBounds.xMax, displayBounds.yMax, Color.black, Color.white);
        DrawAxis();
        DrawGrid();
    }
}
