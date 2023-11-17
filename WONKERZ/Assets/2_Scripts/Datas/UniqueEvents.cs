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
        CIN_FromIntro,
        BNTY_DesertBreakingPot,
        BNTY_DesertWONKERZ,
        BNTY_SawedWays
    };
}