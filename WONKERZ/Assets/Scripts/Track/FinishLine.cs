using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishLine : MonoBehaviour
{
    public bool end_triggered= false;
    public double racetime;
    // Start is called before the first frame update
    void Start()
    {
        end_triggered = false;
        racetime = 0;
        PlayerPrefs.SetInt("racetime", (int)racetime);
    }

    // Update is called once per frame
    void Update()
    {
        if (!end_triggered)
            racetime += Time.deltaTime;
    }

    void OnTriggerEnter(Collider iCol)
    {
        if (!end_triggered)
        {
            PlayerPrefs.SetInt("racetime",(int)racetime);
            PlayerPrefs.Save();
            end_triggered = true;
            SceneManager.LoadScene(Constants.SN_FINISH, LoadSceneMode.Additive); 
        }
    }
}
