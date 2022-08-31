using UnityEngine.UI;

public class UIGraphRenderer : Graphic
{
    public UIGraphView view;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        vh.AddUIVertexStream(view.verts, view.inds);
        view.verts.Clear();
        view.inds.Clear();
    }
}
