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

    // Start is called before the first frame update
    void Start()
    {
        
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
                Destroy(this);
            }
        } else {
            launched = playerDetector.playerInRange;
        }
    }

}
