using UnityEngine;

public class UIResolutionPanelElem : UITextTab
{
    public int idx;
    public RectTransform resScrollView;
    public void ApplyResolution()
    {
        var res = Screen.resolutions[idx];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    public void UpdateView()
    {
        // IMPORTANT toffa : use lossyScale to apply CanvasScaler value to the computation.
        // position already is scaled, sizeDelta and rect are not...
        var view_min = resScrollView.position.y;
        var view_max = view_min - (resScrollView.sizeDelta.y * resScrollView.lossyScale.y);

        var elem_transform = gameObject.GetComponent<RectTransform>();
        var elem_size = elem_transform.rect.height * elem_transform.lossyScale.y;

        var elem_min_bound = elem_size * (idx);

        var content_transform = elem_transform.parent.GetComponent<RectTransform>();
        var content_pos = content_transform.position;

        if (content_pos.y - elem_min_bound - elem_size < view_max)
        {
            content_transform.position -= new Vector3(0, (content_pos.y - elem_min_bound - elem_size - view_max), 0);
        }

        if (content_pos.y - elem_min_bound > view_min)
        {
            content_transform.position -= new Vector3(0, (content_pos.y - elem_min_bound - view_min), 0);
        }
    }
}
