using System.Collections.Generic;

public static class DialogBank
{
    public static readonly string[] TEST_DIALOG= 
    {
        "I've been here forever..."
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
