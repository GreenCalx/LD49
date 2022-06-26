using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
* Interface implemented by UI elements that needs to interact with the InputHelper panel
* > define getHelperInputs() in extending classes to refresh InputHelper panel accordingly
* with inputs/images related to the active ui element.
**/
public interface IUIGarageElement
{
  // Dictonary<Resources path for image, related text>
  public Dictionary<string,string> getHelperInputs();
}