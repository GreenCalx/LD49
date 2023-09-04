using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class ArenaEvent : MonoBehaviour
{
    public bool launched = false;
    public List<GameObject> monstersToKill;
    public UnityEvent triggerOnCompleted;
    public PlayerDetector playerDetector;
    private TrackEvent trackEvent;
    // Start is called before the first frame update
    void Start()
    {
        trackEvent = GetComponent<TrackEvent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (launched)
        {
            // TODO : make it more MVC to avoid stupid foreach
            if (monstersToKill.Where(e => e != null).ToArray().Length == 0)
            {
                triggerOnCompleted.Invoke();
                trackEvent.setSolved();
                Destroy(this);
            }
        } else {
            launched = playerDetector.playerInRange;
        }
    }

}
