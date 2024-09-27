using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wonkerz;
using Mirror;

public class OnlineRenderingUpdater : NetworkBehaviour
{
    public Material skybox;
    public Light sun;

    public void UpdateRenderSettings()
    {
        RenderSettings.skybox = skybox;
        RenderSettings.sun = sun;
    }

    [ClientRpc]
    public void RpcUpdateRenderSettings()
    {
        UpdateRenderSettings();
    }
}
