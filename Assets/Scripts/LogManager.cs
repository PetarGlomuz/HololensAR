using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogManager : MonoBehaviour
{
    public static LogManager Instance;

    public Text LogLabel;
    public string logStr;

    private void Awake()
    {
        // Use this class instance as singleton
        Instance = this;
    }

    string lastStr = "";
    public void ShowLogStr(string log, bool flag = false)
    {
        if(string.IsNullOrEmpty(lastStr))
            logStr = log;
        else
            logStr += "\r\n" + log;

        
        if (!lastStr.Equals(log))
        {
            LogLabel.text = logStr;
        }
        lastStr = log;
    }

    public void ClearLog()
    {
        logStr = "";
        LogLabel.text = logStr;
    }
}
