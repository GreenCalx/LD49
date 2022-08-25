using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPauseMenu : MonoBehaviour, IControllable
{
    private bool menuIsOpened = false;
    // Start is called before the first frame update
    void Start()
    {
    }

    void Awake()
    {
        init();
        Utils.attachControllable<UIPauseMenu>(this);
    }

    private void init()
    {
        deactivateAll();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void IControllable.ProcessInputs(InputManager.InputData Entry) 
    {
       if (Entry.Inputs[Constants.INPUT_START].IsDown)
       {
            if (menuIsOpened)
            {
                deactivateAll();
                pauseGame(false);
            } else {
                activateAll();
                pauseGame(true);
            }

       } 
    }

    private void pauseGame(bool isPaused)
    {
        Time.timeScale = (isPaused ? 0 : 1);
    }

    private void deactivateAll()
    {
        foreach( Transform child in transform )
        {
            child.gameObject.SetActive(false);
        }
        menuIsOpened = false;
    }

    private void activateAll()
    {
        foreach( Transform child in transform )
        {
            child.gameObject.SetActive(true);
        }
        menuIsOpened = true;
    }
}
