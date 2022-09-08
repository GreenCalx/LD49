
public struct Constants
{

  // -- Debug vars --
  public const bool DBG_REPLAYDUMP = true;

  // -- Paths -- 
  public const string FD_SAVEFILES = "./savefiles/";
  public const string unix_FD_SAVEFILES = "";

  // -- Generic Object names extensions --
  public const string EXT_INSTANCE = " (Instance)";

  // -- Scene names --
    public const string SN_SPLASH="SchnibbleSplash";
    public const string SN_TITLE="titlescene";
    public const string SN_MAINGAME="main";
    public const string SN_FINISH="finishscene";
    public const string SN_HUB="HUB";
    public const string SN_LOADING="LoadingScene";


    // -- Global GameObjects --
    public const string GO_CPManager = "CheckpointManager";
    public const string GO_CPRespawn = "respawn";
    public const string GO_FINISH = "finishline";
    public const string GO_PLAYER = "Player";
    public const string GO_HUBCAMERA = "HubCamera";
    public const string GO_SOUNDMANAGER = "SoundManager";
    public const string GO_MANAGERS = "------ MANAGERS ------";
    public const string GO_UIGARAGE = "UIGarage";
    public const string GO_TESTMANAGER = "GARAGEUI_CARTEST";
    
    // -- UI Panels --
    public const string UI_FINISH_SCOREVAL = "TimeTxtVal";

    // -- Inputs --
    public const string INPUT_TURN = "Turn";
    public const string INPUT_JUMP = "Jump";
    public const string INPUT_CANCEL = "Cancel";
    public const string INPUT_UIUPDOWN = "UIUpDown";
    public const string INPUT_RESPAWN = "Respawn";
    public const string INPUT_START = "Start";

    // -- Resources paths --
    public const string RES_ICON_BLANK = "Icons/UI_InputBlank";
    public const string RES_ICON_A = "Icons/UI_InputA";
    public const string RES_ICON_B = "Icons/UI_InputB";
    public const string RES_ICON_Y = "Icons/UI_InputY";

    // -- Layers --
    public const string LYR_UIMESH = "UIMesh";

}