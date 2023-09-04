using System.Collections.Generic;

public static class DialogBank
{
    public static readonly string[] INTRO_DIALOG= 
    {
        "I've been here forever...",
    };

    public static readonly string[] INTRO_CP_DIALOG= 
    {
        "Hello there sleeping beauty...",
        "It's your lucky day ! Ernest sent ME, RICKY, to help you out!",
        "We found your rusted body on the shore and took you back to our garage.",
        "Ernest is trying to fix your body while I invited myself here to fix your mind.",
        "It seems like you've been out of order for a long while, thus I'll help you to get back on track.",
        "Press DPadUp at anytime to teleport back to the last me you met.",
        "There is a few turns ahead. Turning can be quite tricky if you are at full speed. See you in a bit !"
    }; 

    public static readonly string[] INTRO_CP2_DIALOG= 
    {
        "You can control the weight of your car when grounded or in air.",
        "It helps getting over obstacles, land on your wheels or just regain balance.",
        "You can also use it to get up on your own. It can be quite, tricky but you will eventually master it!" 
    }; 

    public static readonly string[] INTRO_CP3_DIALOG= 
    {
        "You can compress your springs to gain more grip by lowering your gravity center.",
        "Upon release, you will jump automatically as the springs extends.",
        "The more you compressed the springs, the higher the jump.",
        "Although it hinders your capacity to climb obstacles or navigate hazardous terrain, it is useful to maintain speed on flat grounds."
    }; 

    public static readonly string[] INTRO_CP4_DIALOG= 
    {
        "Some track sections can be quite long and deadly.",
        "Thus you can plant panels to save your current location and restart from that point at will.",
        "The number of panels is unlimited here, so feel free to use them as much as you need to."
    };

    public static readonly string[] INTRO_CP5_DIALOG= 
    {
        "Nuts are your lifepoints. If you take damage with 0 in the bank you die.",
        "I will re-assemble you at the last checkpoint, but your planted panel will be lost.",
    };

    private static List<string[]> bank;

    static DialogBank()
    {
        // load dialog in bank
        bank = new List<string[]>{
             INTRO_DIALOG,
             INTRO_CP_DIALOG,
             INTRO_CP2_DIALOG,
             INTRO_CP3_DIALOG,
             INTRO_CP4_DIALOG,
             INTRO_CP5_DIALOG
             };
    }

    public static string[] load(int id)
    {
        if ((id < 0) || (id > bank.Count))
            return new string[]{};
        return bank[id] ;
    }

}
