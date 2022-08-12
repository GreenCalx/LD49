using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using IM = InputManager;
using System.IO;
using System.Text;

public class GarageTestDump
{
    private static readonly string timeXinputsSeparator = "__";
    private static readonly string inputXValueSeparator = ":";
    private static readonly string inputXinputSeparator = "..";
    private static readonly string inputStateStartSeparator = "(";
    private static readonly string inputStateXinputeStateSeparator = ",";
    private static readonly string inputStateEndSeparator = ")";

    public static string refDumpName;
    public static string cmpDumpName;

    private static Dictionary<string,IM.InputData> stack;

    public static void initStack()
    {
        stack = new Dictionary<string,IM.InputData>(0);
    }
    public static void addToStack(float currTime, IM.InputData inputData)
    {
        string currentTime = currTime.ToString("N3");
        stack.Add(currentTime, inputData);
    }
    public static void dumpStack(string iFileName)
    {
        FileStream fs = new FileStream(iFileName, FileMode.Create);
        foreach( KeyValuePair<string,IM.InputData> kvp in stack )
        {
            string formated = kvp.Key.ToString();
            formated += timeXinputsSeparator;
            formated += formatInputData(kvp.Value);
            formated += '\n';

            byte[] info = new UTF8Encoding(true).GetBytes(formated);
            fs.Write(info, 0, info.Length);
        }
        fs.Close();
    }

    public static string formatInputData(IM.InputData data)
    {
        string retval = "";
        foreach(KeyValuePair<string,IM.InputState> kvp in data.Inputs)
        {
            retval += kvp.Key;
            retval += inputXValueSeparator;
            retval += formatInputState(kvp.Value);

            retval += inputXinputSeparator;
        }
        return retval;
    }

    private static string formatInputState(IM.InputState iIS)
    {
        string retval = "";
        retval += inputStateStartSeparator;
        
        retval += "IsUp" + inputXValueSeparator + iIS.IsUp.ToString() + inputStateXinputeStateSeparator;
        retval += "IsDown" + inputXValueSeparator + iIS.IsDown.ToString() + inputStateXinputeStateSeparator;
        retval += "Down" + inputXValueSeparator + iIS.Down.ToString() + inputStateXinputeStateSeparator;
        retval += "AxisValue" + inputXValueSeparator + iIS.AxisValue.ToString();

        retval += inputStateEndSeparator;
        return retval;
    }

    public static void compareToStack(string iRefFileName)
    {
        if (!File.Exists(iRefFileName))
        {
            return;
        }
        FileStream fs = File.OpenRead(iRefFileName);
        string content = "";

        byte[] b = new byte[1024];
        UTF8Encoding encoder = new UTF8Encoding(true);
        while (fs.Read(b,0,b.Length) > 0)
        {
            content += encoder.GetString(b);
        }

        Hashtable ref_stack = readStack(content);


    }
    private static Hashtable readStack(string iContent)
    {
        return new Hashtable();
        // TODO
    }

}