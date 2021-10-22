using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// should be in namespace or sm shit but access to tricks not working for some reason ?
public enum WHEEL_LOCATION 
{
        FRONT_RIGHT = 0,
        FRONT_LEFT = 1,
        BACK_RIGHT = 2,
        BACK_LEFT = 3
}

public static class basic_tricks {
        public static readonly bool[] FWHEELIE    = new bool[4]{ false, false, true, true} ;
        public static readonly bool[] BWHEELIE    = new bool[4]{ true, true, false, false} ;
        public static readonly bool[] LWHEELIE    = new bool[4]{ true, false, true, false} ; 
        public static readonly bool[] RWHEELIE    = new bool[4]{ false, true, false, true} ; 
        public static readonly bool[] AIR         = new bool[4]{ false, false, false, false} ; 
        public static readonly bool[] GROUND      = new bool[4]{ true, true, true, true} ; 

        public static bool compare( bool[] iL1, bool[] iL2)
        {
            if ( iL1.Length != iL2.Length)
                return false;
            for( int i=0; i < iL1.Length; i++)
            {
                if ( iL1[i] != iL2[i] )
                    return false;
            }
            return true;
        }

    }

public class TrickTracker : MonoBehaviour
{
    public TrickUI trickUI;
    [HideInInspector]
    public List<WheelTrickTracker> wheels;
    private bool[] wheels_statuses;
    private bool check_tricks;
    // Start is called before the first frame update
    void Start()
    {
        WheelTrickTracker[] arr_wheels = GetComponentsInChildren<WheelTrickTracker>();
        wheels = new List<WheelTrickTracker>();
        wheels.AddRange(arr_wheels);
        if (wheels.Count!=4)
        {
            Debug.LogWarning( wheels.Count + " Wheels in trick tracker. Should have 4.");
        }
        foreach( WheelTrickTracker w in wheels)
        {
            w.tracker = this;
        }

        wheels_statuses = new bool[wheels.Count];
        check_tricks = false;

        if (trickUI == null)
        {
            Debug.LogWarning("TrickUI is missing.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (check_tricks)
            checkTricks();
    }

    public void checkTricks()
    {
        
        // check wheelies 
        if ( basic_tricks.compare(wheels_statuses, basic_tricks.FWHEELIE) )
        {
            trickUI.displayTrick("FRONT WHEELIE");
        }
        else if ( basic_tricks.compare(wheels_statuses, basic_tricks.BWHEELIE) )
        {
            trickUI.displayTrick("BACK WHEELIE");
        }
        else if ( basic_tricks.compare(wheels_statuses, basic_tricks.LWHEELIE) )
        {
            trickUI.displayTrick("LEFT WHEELIE");
        }
        else if ( basic_tricks.compare(wheels_statuses, basic_tricks.RWHEELIE) )
        {
            trickUI.displayTrick("RIGHT WHEELIE");
        }
        else if ( basic_tricks.compare(wheels_statuses, basic_tricks.AIR) )
        {
            trickUI.displayTrick("AIR");
        }
        else if ( basic_tricks.compare(wheels_statuses, basic_tricks.GROUND) )
        {
            trickUI.displayTrick("");
        }

        check_tricks = false;
    }

    public void notify(WheelTrickTracker wtt)
    {
        int wheel_location = (int) wtt.wheel_location;
        wheels_statuses[wheel_location] = wtt.is_grounded;
        check_tricks = true;
    }
}
