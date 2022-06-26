using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGarageColors : UIGarageSelectable, IControllable, IUIGarageElement
{
    public GameObject UIGarageColorPicker_Ref;
    public float selector_latch;

    private GameObject UIGarageColorPicker_Inst;

    private List<UIGaragePickableColor> colors;
    private int i_color;
    private float elapsed_time;

    // Start is called before the first frame update
    void Start()
    {
        Utils.attachControllable<UIGarageColors>(this);
    }

    void OnDestroy() {
        Utils.detachControllable<UIGarageColors>(this);
    }

    void IControllable.ProcessInputs(InputManager.InputData Entry){
        if ( elapsed_time > selector_latch )
        {
            if ( Entry.Inputs["Turn"].AxisValue > 0 )
            {
                deselect(i_color);

                i_color++;
                if ( i_color > colors.Count - 1 )
                { i_color = 0; }
                select(i_color);
                elapsed_time = 0f;
            }
            else if (  Entry.Inputs["Turn"].AxisValue < 0 )
            {
                deselect(i_color);

                i_color--;
                if ( i_color < 0 )
                { i_color = colors.Count - 1; }
                select(i_color);
                elapsed_time = 0f;
            }
        }
        elapsed_time += Time.unscaledDeltaTime;


        if (Entry.Inputs["Jump"].IsDown)
            pick();
        else if (Entry.Inputs["Cancel"].IsDown)
        { 
            quit(); 
            return;
        }
    }

    Dictionary<string,string> IUIGarageElement.getHelperInputs()
    {
        Dictionary<string,string> retval = new Dictionary<string, string>();

        retval.Add(Constants.RES_ICON_A, "APPLY");
        retval.Add(Constants.RES_ICON_B, "BACK");

        return retval;
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void deselect(int index)
    {
        // nothing to do
    }
    private void select(int index)
    {
        // update pos
        GameObject target = colors[i_color].gameObject;
        UIGarageColorPicker_Inst.transform.position = target.transform.position;

        // update color
        Image picker_img = UIGarageColorPicker_Inst.GetComponent<Image>();
        Image target_img = target.GetComponent<Image>();

        picker_img.color = target_img.color;
    }

    public void pick()
    {
        // do change color here
        GameObject target = colors[i_color].gameObject;
        Image target_img = target.GetComponent<Image>();

        Debug.Log(target_img.color);
        PlayerColorManager.Instance.colorize(target_img.color);
    }

    public override void enter(UIGarageSelector uigs)
    {
        base.enter(uigs);

        GameObject.Find(Constants.GO_MANAGERS).GetComponent<InputManager>().SetUnique(this as IControllable);

        colors = new List<UIGaragePickableColor>(GetComponentsInChildren<UIGaragePickableColor>());
        UIGarageColorPicker_Inst = Instantiate(UIGarageColorPicker_Ref, this.transform);

        i_color = 0;
        elapsed_time = 0f;
        select(i_color);
    }

    public override void quit()
    {
        GameObject.Find(Constants.GO_MANAGERS).GetComponent<InputManager>().UnsetUnique(this as IControllable);
        Destroy(UIGarageColorPicker_Inst);

        base.quit();
    }

}
