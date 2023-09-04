using UnityEngine;
using Schnibble;

public class Access
{
    private class AccessCache
    {
        private GameObject GO_MGR;
        private GameObject GO_TSTMGR;
        private GameObject GO_CPMGR;
        private GameObject GO_SOUNDMANAGER;

        private GameObject GO_PLAYER;
        private GameObject GO_UIGARAGE;
        private GameObject GO_UIPLAYER;
        private GameObject GO_PHYSXMATMGR;
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
            if (iHolder == Constants.GO_MANAGERS)
            {
                handler = checkCacheObject(iHolder, ref GO_MGR);
            }
            else if (iHolder == Constants.GO_TESTMANAGER)
            {
                handler = checkCacheObject(iHolder, ref GO_TSTMGR);
            }
            else if (iHolder == Constants.GO_CPManager)
            {
                handler = checkCacheObject(iHolder, ref GO_CPMGR);
            }
            else if (iHolder == Constants.GO_PLAYER)
            {
                handler = checkCacheObject(iHolder, ref GO_PLAYER);
            }
            else if (iHolder == Constants.GO_UIGARAGE)
            {
                handler = checkCacheObject(iHolder, ref GO_UIGARAGE);
            }
            else if (iHolder == Constants.GO_SOUNDMANAGER)
            {
                handler = checkCacheObject(iHolder, ref GO_SOUNDMANAGER);
            }
            else if (iHolder == Constants.GO_PLAYERUI)
            {
                handler = checkCacheObject(iHolder, ref GO_UIPLAYER);
            } else if (iHolder == Constants.GO_PHYSXMATMGR)
            {
                handler = checkCacheObject(iHolder, ref GO_PHYSXMATMGR);
            }
            else
            {
                this.LogWarn("Trying to access : " + iHolder + " as holding object, but is absent from cache.");
                handler = GameObject.Find(iHolder);
            }
            if (!!iComponentIsInChildren)
                return !!handler ? handler.GetComponentInChildren<T>(true) : default(T);
            return !!handler ? handler.GetComponent<T>() : default(T);

        }

        public void invalidate()
        {
            GO_MGR = null;
            GO_TSTMGR = null;
            GO_CPMGR = null;
            GO_PLAYER = null;
            GO_UIGARAGE = null;
            GO_SOUNDMANAGER = null;
            GO_UIPLAYER = null;
            GO_PHYSXMATMGR = null;
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

    public static PlayerController Player()
    {
        return cache.getObject<PlayerController>(Constants.GO_PLAYER, true);
    }

    public static UIGarage UIGarage()
    {
        return cache.getObject<UIGarage>(Constants.GO_UIGARAGE, false);
    }

    public static SoundManagerLoop SoundManager()
    {
        return cache.getObject<SoundManagerLoop>(Constants.GO_SOUNDMANAGER, false);
    }

    public static SceneLoader SceneLoader()
    {
        return cache.getObject<SceneLoader>(Constants.GO_MANAGERS, false);
    }


    public static CollectiblesManager CollectiblesManager()
    {
        return cache.getObject<CollectiblesManager>(Constants.GO_MANAGERS, false);
    }

    public static PlayerCosmeticsManager PlayerCosmeticsManager()
    {
        return cache.getObject<PlayerCosmeticsManager>(Constants.GO_MANAGERS, false);
    }

    public static TrackManager TrackManager()
    {
        return cache.getObject<TrackManager>(Constants.GO_MANAGERS, false);
    }

    public static BountyArray BountyArray()
    {
        return cache.getObject<BountyArray>(Constants.GO_MANAGERS, false);
    }

    public static UITurboAndSaves UITurboAndSaves()
    {
        return cache.getObject<UITurboAndSaves>(Constants.GO_PLAYERUI, true);
    }

    public static UISpeedAndLifePool UISpeedAndLifePool()
    {
        return cache.getObject<UISpeedAndLifePool>(Constants.GO_PLAYERUI, true);
    }

    public static UIBountyUnlocked UIBountyUnlocked()
    {
        return cache.getObject<UIBountyUnlocked>(Constants.GO_PLAYERUI, true);
    }

    public static LevelEntryUI LevelEntryUI()
    {
        return cache.getObject<LevelEntryUI>(Constants.GO_PLAYERUI, true);
    }
    public static UIPowerWheel UIPowerWheel()
    {
        return cache.getObject<UIPowerWheel>(Constants.GO_PLAYERUI, true);
    }
    public static UIWonkerzBar UIWonkerzBar()
    {
        return cache.getObject<UIWonkerzBar>(Constants.GO_PLAYERUI, true);
    }
    public static UICheckpoint UICheckpoint()
    {
        return cache.getObject<UICheckpoint>(Constants.GO_PLAYERUI, true);
    }
    public static PhysicsMaterialManager PhysicsMaterialManager()
    {
        return cache.getObject<PhysicsMaterialManager>(Constants.GO_PHYSXMATMGR, false);
    }

}
