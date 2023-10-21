using System.Collections.Generic;

public static class DialogBank
{
    public static readonly string[] INTRO_DIALOG= 
    {
        "I've been here forever...",
    };

    public static readonly string[] INTRO_CP_DIALOG= 
    {
        "Hello there...",
        "I am Ricky, sole purveyor of the mighty black tar fluid that runs in our pipes.",
        "The headchef of the Garage, Ernest, found the wreckage of your body on the shoreline.",
        "He hauled you back to the Garage and loaded this simulation to help you get back on track while he is twisting bolts.",
        "I will be your guide along the track. ",
        "There is a few turns ahead. Turning can be quite tricky if you are at full speed. See you in a bit !"
    }; 

    public static readonly string[] INTRO_CP2_DIALOG= 
    {
        "You can compress your springs and lower your gravity center.",
        "Upon release, you will jump automatically as the springs extends.",
        "The longer you compressed them, the higher the jump.",
        "When airborne, control the weight to land on your wheels.",
        "Try to jump between waves in the next section to get used to it !"
    }; 

    public static readonly string[] INTRO_CP3_DIALOG= 
    {
        "Weight can also be used grounded to keep balance or navigate on chaotic terrains.",
        "Keep it pushed towards a steep surface you want to keep a grip on."
    }; 

    public static readonly string[] INTRO_CP4_DIALOG= 
    {
        "Some track sections can be quite long and deadly.",
        "Thus you can plant panels to save your current location and restart from that point at will.",
        "The number of panels is unlimited here, so feel free to use them as much as you needs to.",
        "You can also get up on your own by playing with your weight and springs. It can be quite tricky, but with practice you will master it!",
    };

    public static readonly string[] INTRO_CP5_DIALOG= 
    {
        "Nuts are your lifepoints. If you take damage with 0 in the bank you die.",
        "If you get killed, you will respawn at the last triggered Gas Station and the last planted panel will be lost.",
        "The last section is a small arena where you need to defeat every enemies to move forward. Be patient and aim for the target."
    };

    public static readonly string[] ERNEST_DIALOG_0=
    {
        "You're finally awake !",
        "Welcome to Weavers Islands.I am Ernest, the last garagist.",
        "This place used to be a paradise filled with life. But when the Great Compressor arrived in his Junk fortress, everyone left promptly.",
        "My four brothers were abducted shortly after his arrival and locked into their respective realms. Ricky and I are the last living souls on this land.",
        "Alone, it is impossible to fight back. However, now that you are here maybe we can turn things around.",
        "Please find my brothers before the whole island turns into a junkyard for ever.",
        "Reunited, I am sure that we can put an end to this madness.",
        "Try to find Pierre first, he used to run the prosperous Dust City. He is a bit short-tempered, so don't take it personal if he shows some heat.",
        "Drive across the stone bridge and head into the portal that leads to his city."
    };
    public static readonly string[] ERNEST_DIALOG_GARAGEAREA_TUTO0=
    {
        "Before you leave, let me show you the facilities.",
    };
    public static readonly string[] ERNEST_DIALOG_GARAGEAREA_TUTO1=
    {
        "Here is the checklist box. If you accomplish certain objectives, you will unlock cool things to play with.",
    };
    public static readonly string[] ERNEST_DIALOG_GARAGEAREA_TUTO2=
    {
        "There is also the garage itself, which provides car customization.",
        "Unlock skins & other surprises in your checklist to cruise around the world with style!"
    };

    private static List<string[]> bank;

    static DialogBank()
    {
        // load dialog in bank
        bank = new List<string[]>{
             INTRO_DIALOG,                      // 0
             INTRO_CP_DIALOG,                   // 1
             INTRO_CP2_DIALOG,                  // 2
             INTRO_CP3_DIALOG,                  // 3
             INTRO_CP4_DIALOG,                  // 4
             INTRO_CP5_DIALOG,                  // 5
             ERNEST_DIALOG_0,                   // 6
             ERNEST_DIALOG_GARAGEAREA_TUTO0,    // 7
             ERNEST_DIALOG_GARAGEAREA_TUTO1,    // 8
             ERNEST_DIALOG_GARAGEAREA_TUTO2     // 9
             };
    }

    public static string[] load(int id)
    {
        if ((id < 0) || (id > bank.Count))
            return new string[]{};
        return bank[id] ;
    }

}
