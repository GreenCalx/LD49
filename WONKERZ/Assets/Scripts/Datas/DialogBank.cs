using System.Collections.Generic;

public static class DialogBank
{
    public static readonly string[] INTRO_DIALOG= 
    {
        "I've been here forever...",
    };

    public static readonly string[] INTRO_CP_DIALOG= 
    {
        "It's your lucky day ! Ricky's been sent here to help you out !",
        "Ernest warned me that you don't remember anything...",
        "I've placed a few sign to refresh your memories. Read them carefully.",
        "Press DPadUp to teleport back to me."
    }; 

    public static readonly string[] INTRO_CP2_DIALOG= 
    {
        "Well done ! I'll refuel your panels for free.",
        "Don't forget that you can get up again by playing with LB for the ground control."
    }; 

    public static readonly string[] INTRO_CP3_DIALOG= 
    {
        "Nuts are your lifepoints. If you take damage with 0 in the bank you die.",
        "I will re-assemble you at the last checkpoint, but your panels will be lost.",
    }; 

    public static readonly string[] INTRO_CP4_DIALOG= 
    {
        "Nuts are your lifepoints. If you take damage with 0 in the bank you die.",
        "I will re-assemble you at the last checkpoint, but your panels will be lost.",
    };

    public static readonly string[] INTRO_CP5_DIALOG= 
    {
        "Nuts are your lifepoints. If you take damage with 0 in the bank you die.",
        "I will re-assemble you at the last checkpoint, but your panels will be lost.",
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
