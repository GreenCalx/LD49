using UnityEngine;
using UnityEngine.SceneManagement;
using Schnibble;
using System.Collections;
using System.Collections.Generic;

namespace Wonkerz
{
    /// STARTING POINT FOR HUB
    // > ACTIVATES TRICKS AUTO
    public class StartPortal : AbstractCameraPoint
    {
        PlayerController pc = Access.Player();

        init(pc);

            if (enable_tricks)
            activateTricks();

        if (forceSinglePlayer)
        {
            pc.inputMgr = Access.PlayerInputsManager().player1;
        }

            if (!bypassCinematic)
            {
                relocatePlayer(pc);
                entryLevelCinematic.StartCinematic();
                StartCoroutine(waitEntryLevelCinematic(pc));
            }

        }

        void init()
        {
            Access.Player().Freeze();

    void init(PlayerController pc)
    {
        pc.Freeze();

        relocatePlayer(pc);
        if (camera_type != GameCamera.CAM_TYPE.UNDEFINED)
        Access.CameraManager()?.changeCamera(camera_type, false);

        var states = pc.vehicleStates;
        states.SetState(states.states[(int)PlayerVehicleStates.States.Car]);
        
        if (deleteAfterSpawn)
        {
            Destroy(gameObject);
        }

            if (isTutorialStartPortal)
            {
                initTutorial();
            }

        pc.UnFreeze();
    }

        void initTutorial()
        {
            CheckPointManager cpm = Access.CheckPointManager();
            TrackManager tm = Access.TrackManager();
            if (!!cpm && !!tm)
            {
                tm.track_score.selected_diff = DIFFICULTIES.NONE;
                tm.launchTrack(Constants.SN_INTRO);
                cpm.init();
            }
        }

        // Update is called once per frame
        void Update()
        {

        }
        iPC.UnFreeze();
        init(iPC);
    }

    public void relocatePlayer(PlayerController pc)
    {
        PlayerController pc = Access.Player();
        pc.GetTransform().position = transform.position;
        pc.GetTransform().rotation = Quaternion.identity;
        if (facingPoint != null)
        {
            iPC.Freeze();
            while (!entryLevelCinematic.cinematicDone)
            {
                yield return new WaitForSeconds(0.1f);
            }
            iPC.UnFreeze();
            init();
        }

        public void relocatePlayer()
        {
            PlayerController pc = Access.Player();
            pc.GetTransform().position = transform.position;
            pc.GetTransform().rotation = Quaternion.identity;
            if (facingPoint != null)
            {
                pc.GetTransform().LookAt(facingPoint.transform);
            }

            if (pc.GetRigidbody())
            {
                pc.GetRigidbody().velocity = Vector3.zero;
                pc.GetRigidbody().angularVelocity = Vector3.zero;
            }
        }

    public void relocatePlayer()
    {
        relocatePlayer(Access.Player());
    }

    // needed in intro as there is no startline, also for the hub, maybe?
    private void activateTricks()
    {
        TrickTracker tt = Access.Player().gameObject.GetComponent<TrickTracker>();
        if (!!tt)
        {
            tt.activate_tricks = true; // activate default in hub
            tt.init(UIHandle);
        }

    }
}
