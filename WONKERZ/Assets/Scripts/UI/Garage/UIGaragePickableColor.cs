using UnityEngine.UI;

public class UIGaragePickableColor : UIImageTab
{
    override public void activate()
    {
        base.activate();

        PlayerColorManager.Instance.colorize(GetComponent<Image>().color);
    }
}
