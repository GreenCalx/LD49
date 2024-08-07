using UnityEngine;
using Schnibble;
using Schnibble.Managers;

namespace Wonkerz {
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
            private GameObject GO_OFFGAMEMGR;
            private GameObject GO_UIONPLAYER;
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
                } else if ( iHolder == Constants.GO_OFFGAMEMGR)
                {
                    handler = checkCacheObject(iHolder, ref GO_OFFGAMEMGR);
                } else if ( iHolder == Constants.GO_UIONPLAYER)
                {
                    handler = checkCacheObject(iHolder, ref GO_UIONPLAYER);
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
                GO_OFFGAMEMGR = null;
                GO_UIONPLAYER = null; 
            }
        }

        private static AccessCache cache = AccessCache.Instance;

        public static void invalidate()
        {
            cache.invalidate();
        }

        // Generic function to get mgr from manager object.
        public static T Get<T>() {
            if (typeof(T) == typeof(PlayerController)      ) return cache.getObject<T>(Constants.GO_PLAYER      , true)  ;
            if (typeof(T) == typeof(CheckPointManager)     ) return cache.getObject<T>(Constants.GO_CPManager   , false) ;
            if (typeof(T) == typeof(UIGarageTestManager)   ) return cache.getObject<T>(Constants.GO_TESTMANAGER , false) ;
            if (typeof(T) == typeof(PhysicsMaterialManager)) return cache.getObject<T>(Constants.GO_PHYSXMATMGR , false) ;
            if (typeof(T) == typeof(UIGarage)              ) return cache.getObject<T>(Constants.GO_UIGARAGE    , false) ;
            if (typeof(T) == typeof(SoundManagerLoop)      ) return cache.getObject<T>(Constants.GO_SOUNDMANAGER, false) ;
            SchLog.LogError("[Access] Trying to get unknown object : maybe you meant to use GetMgr?");
            return default(T);
        }
        public static T GetMgr<T>() {
            return cache.getObject<T>(Constants.GO_MANAGERS, false);
        }
        public static T GetUI<T>() {
            return cache.getObject<T>(Constants.GO_PLAYERUI, true);
        }

        public static GameSettings GameSettings()
        {
            return cache.getObject<GameSettings>(Constants.GO_MANAGERS, false);
        }

        public static PlayerInputsManager PlayerInputsManager()
        {
            return cache.getObject<PlayerInputsManager>(Constants.GO_MANAGERS, false);
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
            // GameSettings GS = Access.GameSettings();
            // if (GS.IsOnline)
            // {
            //     return cache.getObject<PlayerController>(GS.OnlinePlayerAlias, true);
            // }
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
        public static GameProgressSaveManager GameProgressSaveManager()
        {
            return cache.getObject<GameProgressSaveManager>(Constants.GO_MANAGERS, false);
        }

        public static UISpeedAndLifePool UISpeedAndLifePool()
        {
            return cache.getObject<UISpeedAndLifePool>(Constants.GO_PLAYERUI, true);
        }

        public static UISecondaryFocus UISecondaryFocus()
        {
            if (GameSettings().IsOnline)
                return cache.getObject<UISecondaryFocus>(Constants.GO_UIONPLAYER, true);
            else
                return cache.getObject<UISecondaryFocus>(Constants.GO_PLAYERUI, true);
        }

        public static UIBountyUnlocked UIBountyUnlocked()
        {
            return cache.getObject<UIBountyUnlocked>(Constants.GO_PLAYERUI, true);
        }

        public static LevelEntryUI LevelEntryUI()
        {
            return cache.getObject<LevelEntryUI>(Constants.GO_PLAYERUI, true);
        }
        public static UIWonkerzBar UIWonkerzBar()
        {
            return cache.getObject<UIWonkerzBar>(Constants.GO_PLAYERUI, true);
        }
        public static UICheckpoint UICheckpoint()
        {
            return cache.getObject<UICheckpoint>(Constants.GO_PLAYERUI, true);
        }
        
        public static UITrackEvent UITrackEvent()
        {
            return cache.getObject<UITrackEvent>(Constants.GO_PLAYERUI, true);
        }

        public static PhysicsMaterialManager PhysicsMaterialManager()
        {
            return cache.getObject<PhysicsMaterialManager>(Constants.GO_PHYSXMATMGR, false);
        }

        // ONLINE MODE
        // public static OfflineGameManager OfflineGameManager()
        // {
        //     return cache.getObject<OfflineGameManager>(Constants.GO_OFFGAMEMGR, false);
        // }

        public static UIPlayerOnline UIPlayerOnline()
        {
            return cache.getObject<UIPlayerOnline>(Constants.GO_UIONPLAYER, false);
        }

        public static OnlinePlayerController LocalPlayer()
        {
            return Access.Player()?.GetComponent<OnlinePlayerController>();
        }

}
}
