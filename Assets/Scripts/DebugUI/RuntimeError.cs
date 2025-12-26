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

enum errorType
{
    WARNING,
    ERROR,
    INFO
}

public class RuntimeMsg : MonoBehaviour
{
    public static RuntimeMsg Instance { get; private set; }

    private GameObject errorConsole;
    private GameObject errorConsoleContent;

    /// <summary>
    /// Stores all text objects contained in the console
    /// </summary>
    private List<TextMeshProUGUI> errorObjects = new List<TextMeshProUGUI>();
    private string logFolder;

    private string folderName = "DebugLogs";



    //the '@' is used to tell the compiler that the following is a multiline string
    private const string mrWhite = @"⠀⠀⠀⠀⠀⠀⠀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⡀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⢠⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡇⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⢸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡇⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⢸⡿⠿⠿⠿⠿⠿⠿⠿⠿⠿⠿⠿⠿⠿⠿⢿⣧⠀⠀⠀⠀⠀
⢀⣀⣀⣀⣀⣸⣇⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣀⣸⣿⣀⣀⣀⣀⠀
⠸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠇
⠀⠀⠀⠉⢙⣿⡿⠿⠿⠿⠿⠿⢿⣿⣿⣿⠿⠿⠿⠿⠿⢿⣿⣛⠉⠁⠀⠀
⠀⠀⠀⣰⡟⠉⢰⣶⣶⣶⣶⣶⣶⡶⢶⣶⣶⣶⣶⣶⣶⡆⠉⠻⣧⠀⠀⠀
⠀⠀⠀⢻⣧⡀⠈⣿⣿⣿⣿⣿⡿⠁⠈⢿⣿⣿⣿⣿⣿⠁⠀⣠⡿⠀⠀⠀
⠀⠀⠀⠀⠙⣿⡆⠈⠉⠉⠉⠉⠀⠀⠀⠀⠉⠉⠉⠉⠁⢰⣿⠋⠀⠀⠀⠀
⠀⠀⠀⠀⠀⣿⡇⠀⠀⠀⣠⣶⣶⣶⣶⣶⣶⣄⠀⠀⠀⢸⣿⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠸⣷⡀⠀⠀⣿⠛⠉⠉⠉⠉⠛⣿⠀⠀⢀⣾⠇⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠘⢿⣦⡀⣿⣄⠀⣾⣷⠀⣠⣿⣀⣴⡟⠁⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠙⠻⣿⣿⣿⣿⣿⣿⣿⣿⠟⠁⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠙⠛⠛⠋⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀";

    private void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(this.gameObject);
    }

    private void Start()
    {
        Init();
        logFolder = $"{Path.GetFullPath("./")}{folderName}";
    }

    private void Init()
    {
        errorConsole = DebugUIManager.drawer.errorConsole;
        Button[] buttons = errorConsole.GetComponentsInChildren<Button>();

        foreach (Button b in buttons)
        {
            if (b.CompareTag("clearErrors")) b.onClick.AddListener(ClearErrorConsole);
            if (b.CompareTag("createLog")) b.onClick.AddListener(CreateLogFile);
        }

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

    #endregion

    #region Code to create and format log file
    public void CreateLogFile()
    {

        string res = $"----Debug Log File----\n\nCreated On {Environment.MachineName}\nLocal Machine Time: {DateTime.Now}\nUTC Time: {DateTime.UtcNow}\n\n";

        if (errorObjects.Count > 0)
        {

            res += HandleSectionLogGeneration();

            res += "\n\n\n\n\n\n";

            res += mrWhite;
        }
        else
        {
            res += "Nothing to report... :) Evvivaaaaaaaaaaaa\n";
        }

        HandleCreateLogFile(res);

        Info($"Generated log file at: {logFolder}");
    }

    private void HandleCreateLogFile(string text)
    {
        if (!Directory.Exists(logFolder)) Directory.CreateDirectory(logFolder);
        string filePath = $"{logFolder}{Path.DirectorySeparatorChar}DebugLogFile{Environment.MachineName}.txt";
        //clear the contents of the file
        if (File.Exists(filePath)) File.WriteAllText(filePath, string.Empty);

        FileStream stream = File.Open(filePath, FileMode.OpenOrCreate);

        byte[] arr = Encoding.UTF8.GetBytes(text);

        stream.Write(arr, 0, arr.Length);
        stream.Flush();
        stream.Close();
    }

    /// <summary>
    /// Handles the formatting of errors in the log file
    /// </summary>
    /// <returns></returns>

    private string HandleSectionLogGeneration()
    {
        List<TextMeshProUGUI> errors = new List<TextMeshProUGUI>();
        List<TextMeshProUGUI> warns = new List<TextMeshProUGUI>();
        List<TextMeshProUGUI> infos = new List<TextMeshProUGUI>();


        foreach (TextMeshProUGUI t in errorObjects)
        {
            if (t.text.StartsWith('E')) errors.Add(t);
            else if (t.text.StartsWith('W')) warns.Add(t);
            else infos.Add(t);

        }

        string errorSection = $"---------------Errors: {errors.Count}---------------\n\n";
        foreach (TextMeshProUGUI t in errors)
        {
            errorSection += t.text + "\n\n";
        }

        string warnSection = $"---------------Warnings: {warns.Count}---------------\n\n";
        foreach (TextMeshProUGUI t in warns)
        {
            warnSection += t.text + "\n\n";
        }

        string infoSection = $"---------------Info messages: {infos.Count}---------------\n\n";
        foreach (TextMeshProUGUI t in infos)
        {
            infoSection += t.text + "\n\n";
        }

        infoSection += "\n";

        string res = errorSection + warnSection + infoSection;

        if (res.Contains('<')) res = RemoveRichTags(errorSection + warnSection + infoSection);

        return res;
    }

    /// <summary>
    /// Removes rich text present in the console logs
    /// </summary>
    /// <param name="str">The console log text</param>
    /// <returns></returns>
    private string RemoveRichTags(string str)
    {
        string tmp = str.Remove(str.IndexOf('<'), str.IndexOf('>') + 1 - str.IndexOf('<'));
        //        print(tmp);
        if (tmp.Contains('<')) return RemoveRichTags(tmp);
        return tmp;
    }

    public void ClearErrorConsole()
    {
        foreach (TextMeshProUGUI g in errorObjects)
        {
            Destroy(g.gameObject);
        }
        errorObjects.Clear();
    }

    private static GameObject GenerateDebugText(errorType errorType, string header, string callingClass, string callingMethod, string context = "")
    {
        if (Instance == null) return null;
        if (!Instance.errorConsoleContent)
        {
            Instance.Init();
            return GenerateDebugText(errorType, header, callingClass, callingMethod, context);
        }

        GameObject _textObject = new GameObject(errorType.ToString());
        TextMeshProUGUI textObject = _textObject.AddComponent<TextMeshProUGUI>();
        textObject.SetText(SetErrorText(errorType, header, callingClass, callingMethod, context));

        textObject.color = ErrorColor(errorType);

        textObject.ForceMeshUpdate();

        _textObject.transform.SetParent(Instance.errorConsoleContent.transform);
        Instance.errorObjects.Add(textObject);


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
