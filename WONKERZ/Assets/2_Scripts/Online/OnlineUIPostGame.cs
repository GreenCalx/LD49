using System.Linq;
using System.Collections;

using System.Collections.Generic;
using UnityEngine;
using Schnibble;
using Schnibble.Managers;
using Wonkerz;

using TMPro;

using Mirror;

public class OnlineUIPostGame : NetworkBehaviour, IControllable
{
    [Header("Prefabs Refs")]
    public GameObject prefab_UIPostGameLine;
    [Header("Self Refs")]
    public TextMeshProUGUI timeLbl;
    public Transform postGameLines_Handle;

    [Header("Internal")]
    public bool playerReadyUp = false;
    public OnlinePlayerController OPC;
    private List<UIOnlinePostGameLine> lines = new List<UIOnlinePostGameLine>();

    // Start is called before the first frame update
    void Start()
    {
        playerReadyUp = false;
        if (isClient)
            StartCoroutine(InitCo());
    }

    void OnDestroy()
    {
        if (isClient)
            Access.Player().inputMgr.Detach(this as IControllable);
    }

    void Update()
    {
        updatePostGameTimeLbl();
    }

    private void updatePostGameTimeLbl()
    {
        float trackTime = NetworkRoomManagerExt.singleton.onlineGameManager.postGameTime;
        int trackTime_val_min = (int)(trackTime / 60);
        if (trackTime_val_min<0)
        {
            trackTime_val_min = 0;
        }
        string trackTime_str_min = trackTime_val_min.ToString();
        if (trackTime_str_min.Length<=1)
        {
            trackTime_str_min = "0"+trackTime_str_min;
        }

        int trackTime_val_sec = (int)(trackTime % 60);
        if (trackTime_val_sec<0)
        {
            trackTime_val_min = 0;
        }
        string trackTime_str_sec = trackTime_val_sec.ToString();
        if (trackTime_str_sec.Length<=1)
        {
            trackTime_str_sec = "0"+trackTime_str_sec;
        }

        timeLbl.text = trackTime_str_min +":"+ trackTime_str_sec;
    }

    public void updatePlayerRankingsLbl(OnlineGameManager iOGM)
    {
        int lowest_rank = 1;
        foreach(OnlinePlayerController opc in iOGM.uniquePlayers)
        {
            GameObject newLine = Instantiate(prefab_UIPostGameLine, postGameLines_Handle);
            UIOnlinePostGameLine newLine_asUI = newLine.GetComponent<UIOnlinePostGameLine>();
            if (!!newLine_asUI)
            {
                newLine_asUI.Refresh(opc);
                lines.Add(newLine_asUI);

                if (newLine_asUI.rank > lowest_rank)
                {
                    lowest_rank = newLine_asUI.rank;
                }
            }
        }

        int n_DNF = 0;
        foreach( UIOnlinePostGameLine line in lines)
        {
            if (line.rank > 1)
            {
                line.transform.SetSiblingIndex(line.rank - 1);
            }
            else { // DNF are last
                n_DNF++;
                line.transform.SetSiblingIndex(lowest_rank + n_DNF - 1);
            }

        }

        //winnerLbl.text = NetworkRoomManagerExt.singleton.onlineGameManager.trialManager.dicPlayerTrialFinishPositions.Where(e => e.Value == 1).GetEnumerator().Current.Key.onlinePlayerName;
    }

    IEnumerator InitCo()
    {
        while(OPC==null)    
        {
            OPC = Access.Player()?.GetComponent<OnlinePlayerController>();
            yield return null;
        }
        OPC.self_PlayerController.inputMgr.Attach(this as IControllable);

        while(!playerReadyUp)
        {
            yield return null;
        }
        
        if (isServer)
            NetworkRoomManagerExt.singleton.onlineGameManager.NotifyPlayerIsReady(OPC, true);
        if (isClientOnly)
            OPC.CmdNotifyOGMPlayerReady();
    }

    void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
    {
        if (playerReadyUp)
            return;

        var start = Entry.Get((int)PlayerInputs.InputCode.UIStart) as GameInputButton;
        if (start != null)
        {
            if (start.GetState().down)
            {
                playerReadyUp = true;
            }
        }
    }
}
