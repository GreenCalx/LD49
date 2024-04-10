using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Schnibble;
using Schnibble.Managers;

namespace Wonkerz {

    public class CheckPointManager : MonoBehaviour, IControllable
    {
        public List<GameObject> checkpoints;
        public GameObject race_start;
        public FinishLine finishLine;
        public UICheckpoint ui_cp;

        [HideInInspector]
        public bool player_in_cinematic = false;

        private PlayerController player;

        // SaveStates
        public GameObject saveStateMarkerRef;
        public GameObject saveStateMarkerInst;
        private Vector3 ss_pos = Vector3.zero;
        private Quaternion ss_rot = Quaternion.identity;
        public bool hasSS;
        public int MAX_PANELS = 2;
        public int currPanels { get; set; }

        [HideInInspector]
        public GameObject last_checkpoint;

        [HideInInspector]
        public AbstractCameraPoint last_camerapoint;

        public float timeToForceCPLoad = 2f;
        public float ss_latch = 0.2f;
        private float elapsedSinceLastSS = 0f;
        private float elapsedSinceLastSSLoad = 0f;
        private float respawnButtonDownElapsed = 0f;
        private bool respawnCalled = false;
        public bool saveStateLoaded { get; private set;}

        private bool playerIsFrozen = false;
        private bool anyKeyPressed = false;

        [Header("Internals")]
        public bool playerInGasStation = false;
        public List<Resetable> resetables = new List<Resetable>();

        private UISpeedAndLifePool uiPlayer;
        private TrickTracker TT;
        private TrackManager TrackMGR;

        void Start()
        {
            init();
        }

        public void init()
        {
            if (checkpoints.Count <= 0)
            {
                this.LogWarn("NO checkpoints in CP manager. Should be auto. No CPs at all or Init order of CPs versus CPM ?");
                findCheckpoints();
            }


            player = Access.Player();
            uiPlayer = Access.UISpeedAndLifePool();
            TT = player.GetComponent<TrickTracker>();
            TrackMGR = Access.TrackManager();

            //refreshCameras();
            last_checkpoint = race_start;
            last_camerapoint = race_start.GetComponent<CheckPoint>();
            if (last_camerapoint == null)
            last_camerapoint = race_start.GetComponent<StartPortal>();

            player.inputMgr.Attach(this as IControllable);

            // Init from difficulty
            switch (TrackMGR.track_score.selected_diff)
            {
                case DIFFICULTIES.EASY:
                    MAX_PANELS = Constants.EASY_N_PANELS;
                    break;
                case DIFFICULTIES.MEDIUM:
                    MAX_PANELS = Constants.MEDIUM_N_PANELS;
                    break;
                case DIFFICULTIES.HARD:
                    MAX_PANELS = Constants.HARD_N_PANELS;
                    break;
                case DIFFICULTIES.IRONMAN:
                    MAX_PANELS = Constants.IRONMAN_N_PANELS;
                    foreach(GameObject go in checkpoints)
                    { go.transform.parent.gameObject.SetActive(false); }
                    break;
                default:
                    MAX_PANELS = Constants.EASY_N_PANELS;
                    break;
            }
            currPanels = MAX_PANELS;
            uiPlayer?.updateAvailablePanels(currPanels);

            hasSS = false;
            respawnCalled = false;
            saveStateLoaded = false;
            player_in_cinematic = false;
        }


        void Awake()
        {
        }

        void OnDestroy()
        {
            try{
                Access.PlayerInputsManager().player1.Detach(this as IControllable);
            } catch (NullReferenceException e) {
                this.Log(gameObject.name + " OnDestroy : NULL ref on detachable");
            }
        }

        void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
        {
            if (player_in_cinematic)
            return;

            if (playerIsFrozen)
            anyKeyPressed = currentMgr.IsAnyKeyDown();

            var plantButton = Entry.Get((int)PlayerInputs.InputCode.SaveStatesPlant) as GameInputButton;
            var loadButton = Entry.Get((int)PlayerInputs.InputCode.SaveStatesReturn) as GameInputButton;

            var plant = false;
            var load = false;
            if (plantButton != null) plant = plantButton.GetState().down;
            if (loadButton != null) load = loadButton.GetState().down;

            if (saveStateLoaded && (plant||load))
            {
                return;
            } else { saveStateLoaded = false; }


            if (plant) // SAVE
            {
                if (!playerInGasStation)
                {
                    if (elapsedSinceLastSS >= ss_latch)
                    {
                        putSaveStateDown();
                        elapsedSinceLastSS = 0f;
                    }
                }
                return;
            }
            if ((load)&&(elapsedSinceLastSSLoad>ss_latch)) // LOAD CALL
            {
                if (!respawnCalled)
                {
                    respawnCalled = true;
                    respawnButtonDownElapsed = 0f;
                }
                respawnButtonDownElapsed += Time.deltaTime;
                float fillVal = Mathf.Clamp( respawnButtonDownElapsed / timeToForceCPLoad, 0f, 1f);
                uiPlayer?.updateCPFillImage(fillVal);
            }

            if (respawnCalled) // ACTUAL LOAD
            {
                if (!load) // input released
                {
                    if (respawnButtonDownElapsed>=timeToForceCPLoad)
                    {
                        loadLastCP(false);
                        saveStateLoaded = true;
                    } else {
                        loadLastSaveState();
                        saveStateLoaded = true;
                    }
                    respawnButtonDownElapsed = 0f;
                    respawnCalled = false;
                    uiPlayer?.updateCPFillImage(0f);
                    elapsedSinceLastSSLoad = 0f;
                } else if (respawnButtonDownElapsed>=timeToForceCPLoad) {
                    loadLastCP(false);
                    respawnButtonDownElapsed = 0f;
                    respawnCalled = false;
                    uiPlayer?.updateCPFillImage(0f);
                    elapsedSinceLastSSLoad = 0f;
                    saveStateLoaded = true;
                }
            }
            elapsedSinceLastSS += Time.deltaTime;
            elapsedSinceLastSSLoad += Time.deltaTime;
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void putSaveStateDown()
        {
            if (currPanels>0)
            {
                if (!player.TouchGroundAll())
                return;

                currPanels -= 1;
                ss_pos = player.GetTransform().position;
                ss_rot = player.GetTransform().rotation;
                hasSS = true;

                uiPlayer?.updateAvailablePanels(currPanels);
                if (!!saveStateMarkerRef)
                {
                    if (!!saveStateMarkerInst)
                    Destroy(saveStateMarkerInst);
                    saveStateMarkerInst = Instantiate(saveStateMarkerRef);
                    saveStateMarkerInst.transform.position = ss_pos;
                    saveStateMarkerInst.transform.rotation = ss_rot;
                }
            }
        }

        private void findCheckpoints()
        {
            CheckPoint[] CPs = transform.parent.GetComponentsInChildren<CheckPoint>();
            checkpoints = new List<GameObject>(CPs.Length);
            for (int i = 0; i < CPs.Length; i++)
            {
                CPs[i].subscribeToManager(this);
                if (!checkpoints.Contains(CPs[i].gameObject))
                checkpoints.Add( CPs[i].gameObject );
            }
        }

        public bool subscribe(GameObject iCP)
        {
            if (iCP.GetComponent<CheckPoint>() && !checkpoints.Exists(x => x.gameObject.name == iCP.gameObject.name))
            { checkpoints.Add(iCP); return true; }
            return false;
        }

        public void notifyCP(GameObject iGO, bool setAsLastCPTriggered)
        {
            CheckPoint cp = iGO.GetComponent<CheckPoint>();
            if (!!cp)
            {
                if (last_checkpoint == iGO)
                return;

                if (!setAsLastCPTriggered)
                return;

                last_checkpoint = iGO;
                last_camerapoint = cp;
                currPanels = MAX_PANELS;

                hasSS = false;
                if (!!saveStateMarkerInst)
                Destroy(saveStateMarkerInst);

                uiPlayer?.updateLastCPTriggered(cp.id.ToString());
                uiPlayer?.updateAvailablePanels(currPanels);
                if (!!ui_cp)
                {
                    ui_cp.displayCP(cp);
                }

                if (!!TT && !!TT.trickUI)
                {
                    TrackMGR.addToScore(TT.storedScore);
                    TT.storedScore = 0;
                    TT.trickUI.displayTrackScore(TrackMGR.getTrickScore());
                    TT.trickUI.displayTricklineScore(0);
                }

            }
            else
            this.LogWarn("CheckPointManager: Input GO is not a checkpoint.");
        }

        public void notifyGasStation(GasStation iGasStation)
        {
            currPanels = MAX_PANELS;
        }

        void OnDisable()
        {

        }

        private IEnumerator waitInputToResume(PlayerController iPC)
        {
            iPC.Freeze();
            anyKeyPressed = false;
            yield return new WaitForSeconds(0.2f);
            playerIsFrozen = true;
            while (!anyKeyPressed)
            {
                iPC.GetRigidbody().velocity = Vector3.zero;
                yield return null;
            }
            iPC.UnFreeze();
            playerIsFrozen = false;
        }

        public void OnPlayerRespawn(Transform respawnSource)
        {
            // reset player physx
            Rigidbody rb2d = player.GetRigidbody();
            if (!!rb2d)
            {
                rb2d.velocity = Vector3.zero;
                rb2d.angularVelocity = Vector3.zero;
            }

            // invalidate trick
            if (!!TT && TT.activate_tricks)
            {
                TT.end_line(true);
                TT.storedScore = 0;
                TT.trickUI.displayTricklineScore(0);
            }

            // reset manual camera behind the player
            CameraManager CM = Access.CameraManager();
            if (CM.active_camera!=null)
            {
                ManualCamera manual_cam = CM.active_camera.GetComponent<ManualCamera>();
                if (!!manual_cam)
                { // force realignement
                    manual_cam.resetView();
                }
            }

        }

        public void loadLastSaveState()
        {
            if (!hasSS)
            {
                loadLastCP(false);
            } else {

                player.GetTransform().position = ss_pos;
                player.GetTransform().rotation = ss_rot;
                OnPlayerRespawn(saveStateMarkerInst.transform);
            }

            StartCoroutine(waitInputToResume(player));
        }

        public void loadLastCP(bool iFromDeath = false)
        {
            resetRegisteredResetables();

            // relocate player
            CheckPoint as_cp = last_checkpoint.GetComponent<CheckPoint>();
            if (as_cp != null)
            {
                last_camerapoint = as_cp;
                GameObject respawn = as_cp.getSpawn();

                player.GetTransform().position = respawn.transform.position;
                player.GetTransform().rotation = respawn.transform.rotation;
                OnPlayerRespawn(respawn.transform.parent.transform);

                StartCoroutine(waitInputToResume(player));
            }
            else
            {
                loadRaceStart();
            }
            if (iFromDeath)
            {
                Access.CollectiblesManager().jar.collectedNuts = 0;
                return;
            }

            currPanels = MAX_PANELS;
            Access.CameraManager().TryResetView();
        }

        public void loadRaceStart()
        {
            StartPortal as_sp = last_checkpoint.GetComponent<StartPortal>();
            if (as_sp != null)
            {
                last_camerapoint = as_sp;
            }
            as_sp.relocatePlayer();
            PlayerController pc = player.GetComponent<PlayerController>();
            StartCoroutine(waitInputToResume(pc));
        }

        // expected to be called by Resetables themselves
        public void addResetable(Resetable iR)
        {
            if (!resetables.Contains(iR))
            resetables.Add(iR);
        }

        public void removeResetable(Resetable iR)
        {
            if (resetables.Contains(iR))
            resetables.Remove(iR);
        }

        private void resetRegisteredResetables()
        {
            resetables.RemoveAll((x => x == null));
            foreach(Resetable r in resetables)
            {
                r.load();
            }
        }
    }
}
