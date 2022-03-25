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

    public void ShowLogStr(string log, bool flag = false)
    {
        if(flag)
            logStr = log;
        else
            logStr += "\r\n" + log;

        //logStr += " : " + System.DateTime.Now.ToString();
        LogLabel.text = logStr;
    }

    public void ClearLog()
    {
        logStr = "";
        LogLabel.text = logStr;
    }
}
