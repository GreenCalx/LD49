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
    public OnlinePlayerController OPC;
    private List<UIOnlinePostGameLine> lines = new List<UIOnlinePostGameLine>();

    public override void OnStartClient()
    {
        base.OnStartClient();

        StartCoroutine(InitCo());
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        Access.Player().inputMgr.Detach(this as IControllable);
    }

    void Update()
    {
        updatePostGameTimeLbl();
    }

    private void updatePostGameTimeLbl()
    {
        // We might not have fully loaded but it is fine.
        if (NetworkRoomManagerExt.singleton                   == null) return;
        if (NetworkRoomManagerExt.singleton.onlineGameManager == null) return;

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
        #if false
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
        #endif
    }

    IEnumerator WaitForDependencies() {
        while(OPC==null)    
        {
            OPC = Access.Player()?.GetComponent<OnlinePlayerController>();
            yield return null;
        }
    }

    IEnumerator InitCo()
    {
        yield return StartCoroutine(WaitForDependencies());

        OPC.self_PlayerController.inputMgr.Attach(this as IControllable);
    }

    void IControllable.ProcessInputs(InputManager currentMgr, GameController Entry)
    {
        var start = Entry.Get((int)PlayerInputs.InputCode.UIStart) as GameInputButton;
        if (start != null)
        {
            if (start.GetState().down)
            {
                OPC.CmdModifyReadyState(true);
            }
        }
    }
}
