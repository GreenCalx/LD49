using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Access
{
    private class AccessCache
    {
        public GameObject GO_MGR;
        public GameObject GO_TSTMGR;
        public GameObject GO_CPMGR;

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
        public T getObject<T>(string iHolder)
        {
            GameObject mgr = null;
            if (iHolder==Constants.GO_MANAGERS)
            {
                mgr = !!GO_MGR ? GO_MGR : GameObject.Find(Constants.GO_MANAGERS);
                if (!GO_MGR && !!mgr)
                    GO_MGR = mgr;
            }
            else if (iHolder==Constants.GO_TESTMANAGER)
            {
                mgr = !!GO_TSTMGR ? GO_TSTMGR : GameObject.Find(Constants.GO_TESTMANAGER);
                if (!GO_TSTMGR && !!mgr)
                    GO_TSTMGR = mgr;
            }   
            else if (iHolder==Constants.GO_CPManager)
            {
                mgr = !!GO_CPMGR ? GO_CPMGR : GameObject.Find(Constants.GO_CPManager);
                if (!GO_CPMGR && !!mgr)
                    GO_CPMGR = mgr;
            }
            else
            { 
                Debug.LogWarning("Trying to access : " + iHolder + " as holding object, but is absent from cache.");
                mgr = GameObject.Find(iHolder);
            }

            return !!mgr ? mgr.GetComponent<T>() : default(T);
        }

        public void invalidate()
        {
            GO_MGR = null;
            GO_TSTMGR = null;
            GO_CPMGR = null;
        }
    }

    private static AccessCache cache = AccessCache.Instance;

    public static void invalidate()
    {
        cache.invalidate();
    }

    public static InputManager InputManager()
    {
        return cache.getObject<InputManager>(Constants.GO_MANAGERS);
    }

    public static ResolutionManager ResolutionManager()
    {
        return cache.getObject<ResolutionManager>(Constants.GO_MANAGERS);
    }
    
    public static CameraManager CameraManager()
    {
        return cache.getObject<CameraManager>(Constants.GO_MANAGERS);    
    }

    public static UIGarageTestManager TestManager()
    {
        return cache.getObject<UIGarageTestManager>(Constants.GO_TESTMANAGER);         
    }

    public static CheckPointManager CheckPointManager()
    {
        return cache.getObject<CheckPointManager>(Constants.GO_CPManager);         
    }
}