using UnityEngine;
using Schnibble;
using Schnibble.Managers;
using UnityEngine.SceneManagement;


namespace Wonkerz {

    public static partial class Utils
    {
        public static string GetCurrentSceneName()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            return currentScene.name;
        }

        public static GameObject getPlayerRef()
        {
            return Access.Player().gameObject;
        }

        public static bool isPlayer(GameObject iGO)
        {
            if (iGO.GetComponent<Dummy>()!=null)
            return true;

            SchCarController direct_cc = iGO.GetComponent<SchCarController>();
            if (!!direct_cc)
            return true;
            // can also be wheels
            CarColorizable carpart = iGO.GetComponent<CarColorizable>();
            if (!!carpart)
            return true;

            if (iGO.transform.parent == null)
            return false;

            SchCarController[] parent_cc = iGO.GetComponentsInParent<SchCarController>();
            if (parent_cc != null && parent_cc.Length>0)
            return true;

            return false;
        }

        public static bool colliderIsPlayer(Collider iCollider)
        {
            if (!!iCollider.transform.parent)
            return !!iCollider.transform.gameObject.GetComponentInParent<PlayerController>();
            else
            return false;
        }

        public static bool collisionIsPlayer(Collision iCollision)
        {
            return !!colliderIsPlayer(iCollision.collider);
        }

        public static float lerp(float a, float b, float f)
        {
            return a * (1f - f) + (b * f);
        }
    }
}
