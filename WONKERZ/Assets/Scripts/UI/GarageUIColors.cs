using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GarageUIColors : GarageUISelectable
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
    }

    // Update is called once per frame
    void Update()
    {
        if (!is_active)
            return;

        if ( elapsed_time > selector_latch )
        {
            if ( Input.GetAxis("Horizontal") > 0 )
            {
                deselect(i_color);

                i_color++;
                if ( i_color > colors.Count - 1 )
                { i_color = 0; }
                select(i_color);
                elapsed_time = 0f;
            }
            else if ( Input.GetAxis("Horizontal") < 0 )
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


        if (Input.GetButtonDown("Submit"))
            pick();
        else if (Input.GetButtonDown("Cancel"))
        { parent.quitSubMenu(); return;}
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

    public override void enter()
    {
        base.enter();
        colors = new List<UIGaragePickableColor>(parent.GetComponentsInChildren<UIGaragePickableColor>());
        UIGarageColorPicker_Inst = Instantiate(UIGarageColorPicker_Ref, this.transform);

        i_color = 0;
        elapsed_time = 0f;
        select(i_color);
    }

    public override void quit()
    {
        Destroy(UIGarageColorPicker_Inst);

        base.quit();
    }

}
