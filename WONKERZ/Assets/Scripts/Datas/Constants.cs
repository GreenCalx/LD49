using System;

public struct Constants
{
    // -- Tweaks --
    public const int EASY_N_PANELS      = Int32.MaxValue;
    public const int MEDIUM_N_PANELS    = 3;
    public const int HARD_N_PANELS      = 0;
    public const int IRONMAN_N_PANELS   = 3;

    // -- Debug vars --
    public const bool DBG_REPLAYDUMP = false;

    // -- Paths --
    public const string FD_SAVEFILES = "./savefiles/";
    public const string unix_FD_SAVEFILES = "";
    public const string FD_BOUNTYMATRIX = "bountymatrix";

    // -- Generic Object names extensions --
    public const string EXT_INSTANCE = " (Instance)";

    // -- Scene names --
    public const string SN_SPLASH = "SchnibbleSplash";
    public const string SN_TITLE = "titlescene";
    public const string SN_MAINGAME = "main";
    public const string SN_FINISH = "finishscene";
    public const string SN_HUB = "HUB";
    public const string SN_LOADING = "LoadingScene";
    public const string SN_INTRO = "Intro";
    public const string SN_DESERT_TOWER = "desert_tower";
    public const string SN_GROTTO_TRACK = "GrottoTrack";
    public const string SN_WATERWORLD_TRACK = "WaterWorldTrack";
    public const string SN_SKYCASTLE_TRACK = "SkyCastleTrack";
    public const string SN_JUNKYARD_TRACK = "JunkyardTrack";
    public static readonly string[] SN_TRACKS = { SN_DESERT_TOWER, SN_GROTTO_TRACK, SN_WATERWORLD_TRACK, SN_SKYCASTLE_TRACK, SN_JUNKYARD_TRACK};



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
    public const string GO_PLAYERUI = "PlayerUI";

    // -- UI Panels --
    public const string UI_FINISH_SCOREVAL = "TimeTxtVal";

    // -- Inputs --
    public const string INPUT_ACCEL = "Accelerator";
    public const string INPUT_TURN = "Turn";
    public const string INPUT_JUMP = "Jump";
    public const string INPUT_CANCEL = "Cancel";
    public const string INPUT_UIUPDOWN = "UIUpDown";
    public const string INPUT_START = "Start";
    public const string INPUT_SAVESTATES = "SaveStates";
    public const string INPUT_TURBO = "Turbo";
    public const string INPUT_CAMERACHANGE = "CameraChange";
    public static readonly string[] INPUT_AXIS = {INPUT_ACCEL, INPUT_TURN, INPUT_SAVESTATES}; // No cam input innit atm
    public static readonly string[] INPUT_BTNS = {INPUT_JUMP, INPUT_CANCEL, INPUT_TURBO, INPUT_START, INPUT_CAMERACHANGE};

    // -- Resources paths --
    public const string RES_ICON_BLANK = "Icons/UI_InputBlank";
    public const string RES_ICON_A = "Icons/UI_InputA";
    public const string RES_ICON_B = "Icons/UI_InputB";
    public const string RES_ICON_Y = "Icons/UI_InputY";

    // -- Layers --
    public const string LYR_UIMESH = "UIMesh";
    public const string LYR_NOPLAYERCOL = "No Player Collision";

}
