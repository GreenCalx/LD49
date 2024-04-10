using UnityEngine;
using Schnibble;
using Schnibble.Managers;

namespace Wonkerz
{

    public class Hookable : MonoBehaviour, IControllable
    {
        private PlayerController ccPlayer;
        public GameObject hook;
        public Vector3 D;

        // Start is called before the first frame update
        void Start()
        {
            Access.Player().inputMgr.Attach(this as IControllable);
            ccPlayer = null;
            hook.SetActive(false);
        }

        private void OnDestroy()
        {
            Access.Player().inputMgr.Detach(this as IControllable);
        }

        void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
        {
            hook.SetActive((Entry.Get((int)PlayerInputs.InputCode.Grapin) as GameInputButton).GetState().heldDown);
            if (!!ccPlayer)
            ccPlayer.IsHooked = (Entry.Get((int)PlayerInputs.InputCode.Grapin) as GameInputButton).GetState().heldDown;
        }

        // Update is called once per frame
        void Update()
        {
            if (!!ccPlayer && ccPlayer.IsHooked)
            {
                D = (transform.position - ccPlayer.transform.position);
                hook.transform.position = transform.position - D / 2;
                hook.transform.localScale = new Vector3(1, D.magnitude / 2, 1) / 10;
                hook.transform.rotation = Quaternion.FromToRotation(transform.up, D);
            }
        }

        public void OnTriggerEnter(Collider iCol)
        {
            if (null == ccPlayer)
            ccPlayer = iCol.GetComponent<PlayerController>();
        }
        public void OnTriggerStay(Collider iCol)
        {
            if (null == ccPlayer)
            ccPlayer = iCol.GetComponent<PlayerController>();
        }
        public void OnTriggerExit(Collider iCol)
        {
            if (!!ccPlayer && !ccPlayer.IsHooked)
            ccPlayer = iCol.GetComponent<PlayerController>();
        }
    }
}
