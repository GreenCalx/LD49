using UnityEngine;
using Schnibble;

public class ObjectDisabler : MonoBehaviour
{

    public GameObject to_disable;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider iCol)
    {
        SchCarController player = iCol.GetComponent<SchCarController>();
        if (!!player)
        {
            to_disable.SetActive(false);
        }
    }

    public void reenable()
    {
        to_disable.SetActive(true);
    }
}
