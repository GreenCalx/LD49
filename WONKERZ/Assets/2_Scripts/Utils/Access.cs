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
                }
                else if (iHolder == Constants.GO_PHYSXMATMGR)
                {
                    handler = checkCacheObject(iHolder, ref GO_PHYSXMATMGR);
                }
                else if (iHolder == Constants.GO_OFFGAMEMGR)
                {
                    handler = checkCacheObject(iHolder, ref GO_OFFGAMEMGR);
                }
                else if (iHolder == Constants.GO_UIONPLAYER)
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
        public static T Get<T>()
        {
            if (typeof(T) == typeof(PlayerController))       return cache.getObject<T>(Constants.GO_PLAYER      , true);
            if (typeof(T) == typeof(CheckPointManager))      return cache.getObject<T>(Constants.GO_CPManager   , false);
            if (typeof(T) == typeof(UIGarageTestManager))    return cache.getObject<T>(Constants.GO_TESTMANAGER , false);
            if (typeof(T) == typeof(PhysicsMaterialManager)) return cache.getObject<T>(Constants.GO_PHYSXMATMGR , false);
            if (typeof(T) == typeof(UIGarage))               return cache.getObject<T>(Constants.GO_UIGARAGE    , false);
            if (typeof(T) == typeof(SoundManagerLoop))       return cache.getObject<T>(Constants.GO_SOUNDMANAGER, false);
            SchLog.LogError("[Access] Trying to get unknown object : maybe you meant to use GetMgr?");
            return default(T);
        }
        public static T GetMgr<T>()
        {
            return cache.getObject<T>(Constants.GO_MANAGERS, false);
        }

        public static T GetUI<T>()
        {
            if (typeof(T) == typeof(UIGarage)) return cache.getObject<T>(Constants.GO_UIGARAGE, false);

            return cache.getObject<T>(Constants.GO_PLAYERUI, true);
        }

        public static T GetOnlineUI<T>() {
            return cache.getObject<T>(Constants.GO_UIONPLAYER, true);
        }

        public static UIGarageTestManager     TestManager            () {return Get   <UIGarageTestManager    >();}
        public static CheckPointManager       CheckPointManager      () {return Get   <CheckPointManager      >();}
        public static PlayerController        Player                 () {return Get   <PlayerController       >();}
        public static SoundManagerLoop        SoundManager           () {return Get   <SoundManagerLoop       >();}
        public static PhysicsMaterialManager  PhysicsMaterialManager () {return Get   <PhysicsMaterialManager >();}
        public static GameSettings            GameSettings           () {return GetMgr<GameSettings           >();}
        public static PlayerInputsManager     PlayerInputsManager    () {return GetMgr<PlayerInputsManager    >();}
        public static CameraManager           CameraManager          () {return GetMgr<CameraManager          >();}
        public static SceneLoader             SceneLoader            () {return GetMgr<SceneLoader            >();}
        public static CollectiblesManager     CollectiblesManager    () {return GetMgr<CollectiblesManager    >();}
        public static PlayerCosmeticsManager  PlayerCosmeticsManager () {return GetMgr<PlayerCosmeticsManager >();}
        public static TrackManager            TrackManager           () {return GetMgr<TrackManager           >();}
        public static BountyArray             BountyArray            () {return GetMgr<BountyArray            >();}
        public static GameProgressSaveManager GameProgressSaveManager() {return GetMgr<GameProgressSaveManager>();}

        public static UISecondaryFocus   UISecondaryFocus    () {
            if (GameSettings().isOnline) return GetOnlineUI<UISecondaryFocus>();
            else                         return GetUI      <UISecondaryFocus>();
        }
        //public static UIPlayerOnline     UIPlayerOnline      () {return GetOnlineUI<UIPlayerOnline    >();}
        public static UISpeedAndLifePool UISpeedAndLifePool  () {return GetUI      <UISpeedAndLifePool>();}
        public static UIGarage           UIGarage            () {return GetUI      <UIGarage          >(); }
        public static UIBountyUnlocked   UIBountyUnlocked    () {return GetUI      <UIBountyUnlocked  >();}
        public static LevelEntryUI       LevelEntryUI        () {return GetUI      <LevelEntryUI      >();}
        public static UIWonkerzBar       UIWonkerzBar        () {return GetUI      <UIWonkerzBar      >();}
        public static UICheckpoint       UICheckpoint        () {return GetUI      <UICheckpoint      >();}
        public static UITrackEvent       UITrackEvent        () {return GetUI      <UITrackEvent      >();}


}
}
