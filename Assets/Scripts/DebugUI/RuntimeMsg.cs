using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Data;
using System.Diagnostics;
using Button = UnityEngine.UI.Button;
using Unity.VisualScripting;

enum errorType
{
    WARNING,
    ERROR,
    INFO
}

public static class RuntimeMsg
{

    private static GameObject errorConsole;
    private static GameObject errorConsoleContent;

    /// <summary>
    /// Stores all text objects contained in the console
    /// </summary>
    public static List<TextMeshProUGUI> errorObjects { get; private set; } = new List<TextMeshProUGUI>();


    private static void Init()
    {
        errorConsole = DebugUIManager.drawer.errorConsole;
        errorConsoleContent = errorConsole.GetComponentInChildren<VerticalLayoutGroup>().gameObject;
    }

    #region Code to display shit in the console

    public static void Info(string Message, string additionalInfo = "")
    {
        //need to call this here, cannot even put in another method
        //as the frame referenced can be diraclty accessed reliably from the function invoked and not in sub funcitons
        StackTrace stackTrace = new StackTrace(true);
        string callingClass = stackTrace.GetFrame(1).GetMethod().DeclaringType.ToString();
        string callingMethod = stackTrace.GetFrame(1).GetMethod().Name;

        string header = Message;
        GenerateDebugText(errorType.INFO, header, callingClass, callingMethod, additionalInfo);
    }

    public static void Warning(string Message, string additionalInfo = "")
    {
        //need to call this here, cannot even put in another method
        //as the frame referenced can be diraclty accessed reliably from the function invoked and not in sub funcitons
        StackTrace stackTrace = new StackTrace(true);
        string callingClass = stackTrace.GetFrame(1).GetMethod().DeclaringType.ToString();
        string callingMethod = stackTrace.GetFrame(1).GetMethod().Name;


        string header = Message;
        GenerateDebugText(errorType.WARNING, header, callingClass, callingMethod, additionalInfo);
    }

    public static void Error(Exception e)
    {
        //need to call this here, cannot even put in another method
        //as the frame referenced can be diraclty accessed reliably from the function invoked and not in sub funcitons
        StackTrace stackTrace = new StackTrace(true);
        string callingClass = stackTrace.GetFrame(1).GetMethod().DeclaringType.ToString();
        string callingMethod = stackTrace.GetFrame(1).GetMethod().Name;

        string header = $"{new string((from c in e.Message where !char.IsControl(c) select c).ToArray())}";
        string additionalInfo = e.ToString();

        GenerateDebugText(errorType.ERROR, header, callingClass, callingMethod, additionalInfo);
    }

    public static void Error(Exception e, string context)
    {
        //need to call this here, cannot even put in another method
        //as the frame referenced can be diraclty accessed reliably from the function invoked and not in sub funcitons
        StackTrace stackTrace = new StackTrace(true);
        string callingClass = stackTrace.GetFrame(1).GetMethod().DeclaringType.ToString();
        string callingMethod = stackTrace.GetFrame(1).GetMethod().Name;

        string header = $"{new string((from c in e.Message where !char.IsControl(c) select c).ToArray())}";

        GenerateDebugText(errorType.ERROR, header, callingClass, callingMethod, context);
    }

    public static void Error(string text)
    {
        //need to call this here, cannot even put in another method
        //as the frame referenced can be diraclty accessed reliably from the function invoked and not in sub funcitons
        StackTrace stackTrace = new StackTrace(true);
        string callingClass = stackTrace.GetFrame(1).GetMethod().DeclaringType.ToString();
        string callingMethod = stackTrace.GetFrame(1).GetMethod().Name;

        string header = $"{new string((from c in text where !char.IsControl(c) select c).ToArray())}";
        GenerateDebugText(errorType.ERROR, header, callingClass, callingMethod);
    }

    public static void Error(string text, string context)
    {
        //need to call this here, cannot even put in another method
        //as the frame referenced can be diraclty accessed reliably from the function invoked and not in sub funcitons
        StackTrace stackTrace = new StackTrace(true);
        string callingClass = stackTrace.GetFrame(1).GetMethod().DeclaringType.ToString();
        string callingMethod = stackTrace.GetFrame(1).GetMethod().Name;

        string header = $"{new string((from c in text where !char.IsControl(c) select c).ToArray())}";

        GenerateDebugText(errorType.ERROR, header, callingClass, callingMethod, context);
    }

    private static GameObject GenerateDebugText(errorType errorType, string header, string callingClass, string callingMethod, string context = "")
    {
        if (!errorConsoleContent)
        {
            Init();
            return GenerateDebugText(errorType, header, callingClass, callingMethod, context);
        }

        GameObject _textObject = new GameObject(errorType.ToString());
        TextMeshProUGUI textObject = _textObject.AddComponent<TextMeshProUGUI>();
        textObject.SetText(SetErrorText(errorType, header, callingClass, callingMethod, context));

        textObject.color = ErrorColor(errorType);

        textObject.ForceMeshUpdate();

        _textObject.transform.SetParent(errorConsoleContent.transform);
        errorObjects.Add(textObject);


        return _textObject;
    }

    private static Color ErrorColor(errorType errorType)
    {
        switch (errorType)
        {
            case errorType.WARNING: return Color.yellow;
            case errorType.ERROR: return Color.red;
            default: return Color.white;
        }
    }

    private static string SetErrorText(errorType errorType, string header, string callingClass, string callingMethod, string context = "")
    {
        string control = "\n<size=55%><line-indent=4%>";
        string res;
        switch (errorType)
        {
            case errorType.WARNING: res = "Warning: "; break;
            case errorType.ERROR: res = "Error: "; break;
            default:
                res = "Info: ";
                break;
        }

        res += $"{callingClass}.{callingMethod}: {header}{control}{context}";
        return res;
    }
    
    #endregion
}
