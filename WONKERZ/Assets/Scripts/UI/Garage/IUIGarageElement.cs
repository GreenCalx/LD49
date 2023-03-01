using System.Collections.Generic;

/**
* Interface implemented by UI elements that needs to interact with the InputHelper panel
* > define getHelperInputs() in extending classes to refresh InputHelper panel accordingly
* with inputs/images related to the active ui element.
**/
public interface IUIGarageElement
{
    public struct UIGarageHelperValue
    {
        public string imgName;
        public string txt;

        public UIGarageHelperValue(string imgName, string txt)
        {
            this.imgName = imgName;
            this.txt = txt;
        }
    }
    // Dictonary<Resources path for image, related text>
    public List<UIGarageHelperValue> getHelperInputs();
}
