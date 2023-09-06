using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

/**
*   TODO : This is a subtype of TrackEvent, should derive from it directly and spec its type
*   for better interop with UITrackEvent.
*/
public class ArenaEvent : MonoBehaviour
{
    [Header("MAND")]
    [HideInInspector]
    public bool launched = false;
    public List<GameObject> monstersToKill;
    public UnityEvent triggerOnCompleted;
    public PlayerDetector playerDetector;
    private TrackEvent trackEvent;

    [Header("UI")]
    public string eventName;
    public string hintText;
    [HideInInspector]

    private int numberOfMonstersAtStart = 0;
    private int killedMonsters = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        trackEvent = GetComponent<TrackEvent>();
        numberOfMonstersAtStart = monstersToKill.Count;
        killedMonsters = 0;
    }

    private void updateHintStatus()
    {
        Access.UITrackEvent().refreshStatus(getHintStatus());
    }

    private string getHintStatus()
    {
        return killedMonsters.ToString()  + "/" + numberOfMonstersAtStart.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if (launched)
        {
            updateHintStatus();
            // TODO : make it more MVC to avoid stupid foreach
            killedMonsters =  numberOfMonstersAtStart
                                - monstersToKill.Where(e => e != null).ToArray().Length;

            if (killedMonsters >= numberOfMonstersAtStart)
            {
                Access.UITrackEvent().hide();
                triggerOnCompleted.Invoke();
                trackEvent.setSolved();
                Destroy(this);
            }

        } else {
            launched = playerDetector.playerInRange;
            if (launched)
            {
                Access.UITrackEvent().show(eventName, hintText, getHintStatus());
            }
        }
    }

}
