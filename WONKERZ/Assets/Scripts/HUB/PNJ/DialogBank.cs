using System.Collections;
using System.Collections.Generic;
using System;

public static class DialogBank
{
    public static readonly string[] TEST_DIALOG= 
    {
        "Welcome to WONKERZ.",
        "I am a debug NPC.",
        "Explore the HUB and find portals."
    };


    private static List<string[]> bank;

    static DialogBank()
    {
        // load dialog in bank
        bank = new List<string[]>{
             TEST_DIALOG 
             };
    }

    public static string[] load(int id)
    {
        if ((id < 0) || (id > bank.Count))
            return new string[]{};
        return bank[id] ;
    }

}
