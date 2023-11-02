using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Schnibble;

public class UIDialog : MonoBehaviour
{
    [Header("MAND")]
    public TextMeshProUGUI message;
    public TextMeshProUGUI header;

    public GameObject GoNextHandle;

    [Header("Tweaks")]
    public float wait_time_to_print_char = 0.1f;
    [HideInInspector]
    public string overflowing_text = "";
    [HideInInspector]
    public bool overflows = false;
    public bool keepGoNextTooltipVisible = true;

    private string __msg_to_display;
    private int __msg_size;
    private int __curr_msg_index;
    private float __timer;
    private bool __text_fully_displayed;
    private bool __overflow_checked;

    // Start is called before the first frame update
    void Awake()
    {
        __msg_to_display = "";
        message.text = __msg_to_display;
        __curr_msg_index = 0;
        __text_fully_displayed = false;
        __overflow_checked = false;
        if (keepGoNextTooltipVisible)
            GoNextHandle.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        __timer += Time.deltaTime;
        if (__timer > wait_time_to_print_char)
        {
            if (__curr_msg_index <= __msg_size)
            {
                message.text = __msg_to_display.Substring(0, __curr_msg_index);
                __curr_msg_index++;
                __text_fully_displayed = (__curr_msg_index > __msg_size);
            }
            __timer -= wait_time_to_print_char;
        }
        if (message_is_displayed())
        {
            GoNextHandle.SetActive(true);
            if (!__overflow_checked)
                updateVerticalOverflow();
        }
        else {
            if (!keepGoNextTooltipVisible)
                GoNextHandle.SetActive(false);
        }
            
    }

    public void force_display()
    {
        message.text = __msg_to_display;
        __curr_msg_index = __msg_size;
        __text_fully_displayed = true;
    }

    public bool message_is_displayed()
    {
        return __text_fully_displayed;
    }

    public bool has_a_message_to_display()
    {
        return (__msg_to_display.Length != 0);
    }

    public void display(string iHeader, string iText)
    {
        displayHeader(iHeader);
        __msg_to_display = iText;
        __msg_size = iText.Length;
        __curr_msg_index = 0;
        __text_fully_displayed = false;
        __overflow_checked = false;
    }

    private void displayHeader(string iHeaderTxt)
    {
        if (header==null)
            return;
        
        header.text = iHeaderTxt;
    }

    public void updateVerticalOverflow()
    {
        Canvas.ForceUpdateCanvases();
        // int n_visible = message.cachedTextGenerator.characterCountVisible;
        // overflows = n_visible >= 0 ? n_visible < __msg_to_display.Length : false;
        // if (overflows)
        //     overflowing_text = __msg_to_display.Substring(n_visible);
        __overflow_checked = true;
    }
}
