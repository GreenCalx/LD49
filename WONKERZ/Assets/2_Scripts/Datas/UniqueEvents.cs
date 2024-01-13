using System;

/**
*   Playthrough wide events that occures only once.
*   It contains :
*       > Cinematics that continues the narrative
*           > Ex: Cinematic coming from intro
*       > Meta Progress of the player involving changes in the overworld
            > Ex : Freeing the garagist Pierre and thus see him in the overworld
*       > BountyMatrix unlocks.
            > Ex : BreakingPot chase
*
*   A Note on the relation to BountyMatrix:
*       A TrackEvent 
*   
*/
public struct UniqueEvents
{

    /// Game Progress 
    public const string GP_IntroComplete = "GP_IntroComplete";
    public const string GP_DesertComplete = "GP_DesertComplete";
    public const string GP_IceParkComplete = "GP_IceParkComplete";
    public const string GP_WaterWorldComplete = "GP_WaterWorldComplete";
    public const string GP_SkyCastleComplete = "GP_SkyCastleComplete";
    public const string GP_JunkyardComplete = "GP_JunkyardComplete";
    /// Unique Cinematics
    public const string CIN_FromIntro = "CIN_FromIntro";

    /// Bounty Matrix unlock
    public const string BNTY_DesertBreakingPot = "BNTY_DesertBreakingPot";
    public const string BNTY_DesertWONKERZ = "BNTY_DesertWonkerz";
    public const string BNTY_SawedWays = "BNTY_SawedWays";

    public enum UEVENTS
    {
        NONE,
        GP_IntroComplete,
        GP_DesertComplete,
        GP_IceParkComplete,
        GP_WaterWorldComplete,
        GP_SkyCastleComplete,
        GP_JunkyardComplete,
        CIN_FromIntro,
        BNTY_DesertBreakingPot,
        BNTY_DesertWONKERZ,
        BNTY_SawedWays
        
    };

    public static string GetEventName(UniqueEvents.UEVENTS iEvent)
    {
        return Enum.GetName(typeof(UniqueEvents.UEVENTS), iEvent);
    }
}