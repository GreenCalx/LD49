using UnityEngine;

public class ShowPB : MonoBehaviour
{
    public TMPro.TextMeshProUGUI Text;

    // Start is called before the first frame update
    void Start()
    {

        var pb = PlayerPrefs.GetInt("pb", 0);
        int pb_val_min = (int)(pb / 60);
        int pb_val_sec = (int)(pb % 60);
        Text.SetText("Current PB : " + pb_val_min.ToString() + " m " + pb_val_sec.ToString() + " s ");

    }

    // Update is called once per frame
    void Update()
    {

    }
}
