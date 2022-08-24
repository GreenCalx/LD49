using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Access
{
    private class AccessCache
    {
        private GameObject GO_MGR;
        private GameObject GO_TSTMGR;
        private GameObject GO_CPMGR;

        private GameObject GO_PLAYER;

/* IF WE WANT TO ECO GetComponent<> call..
        public InputManager         IM;
        public ResolutionManager    RM;
        public CameraManager        CM;
        public UIGarageTestManager  UIGTM;
        public CheckPointManager    CPM;
*/      
        private static AccessCache inst;
        public static AccessCache Instance
        {
            get { return inst ?? (inst = new AccessCache()); }
            private set { inst = value; }
        }
        public GameObject checkCacheObject(string iHolder, ref GameObject iStorage)
        {
            GameObject handler = null;
            handler = !!iStorage ? iStorage : GameObject.Find(iHolder);
            if (!iStorage && !!handler)
                iStorage = handler;
            return handler;
        }
        public T getObject<T>(string iHolder, bool iComponentIsInChildren)
        {
            GameObject handler = null;
            if (iHolder==Constants.GO_MANAGERS)
            {
                handler = checkCacheObject( iHolder, ref GO_MGR);
            }
            else if (iHolder==Constants.GO_TESTMANAGER)
            {
                handler = checkCacheObject( iHolder, ref GO_TSTMGR);
            }   
            else if (iHolder==Constants.GO_CPManager)
            {
                handler = checkCacheObject( iHolder, ref GO_CPMGR);
            }
            else if (iHolder==Constants.GO_PLAYER)
            {
                handler = checkCacheObject(iHolder, ref GO_PLAYER);
            }
            else
            { 
                Debug.LogWarning("Trying to access : " + iHolder + " as holding object, but is absent from cache.");
                handler = GameObject.Find(iHolder);
            }
            if (!!iComponentIsInChildren)
                return !!handler ? handler.GetComponentInChildren<T>() : default(T);
            return !!handler ? handler.GetComponent<T>() : default(T);

        }

        public void invalidate()
        {
            GO_MGR = null;
            GO_TSTMGR = null;
            GO_CPMGR = null;
            GO_PLAYER = null;
        }
    }

    private static AccessCache cache = AccessCache.Instance;

    public static void invalidate()
    {
        cache.invalidate();
    }

    public static InputManager InputManager()
    {
        return cache.getObject<InputManager>(Constants.GO_MANAGERS, false);
    }

    public static ResolutionManager ResolutionManager()
    {
        return cache.getObject<ResolutionManager>(Constants.GO_MANAGERS, false);
    }
    
    public static CameraManager CameraManager()
    {
        return cache.getObject<CameraManager>(Constants.GO_MANAGERS, false);    
    }

    public static UIGarageTestManager TestManager()
    {
        return cache.getObject<UIGarageTestManager>(Constants.GO_TESTMANAGER, false);         
    }

    public static CheckPointManager CheckPointManager()
    {
        return cache.getObject<CheckPointManager>(Constants.GO_CPManager, false);         
    }

    public static CarController Player()
    {
        return cache.getObject<CarController>(Constants.GO_PLAYER, true);
    }
}