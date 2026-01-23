using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ErrorConsoleHandler : MonoBehaviour
{
    [SerializeField] private Button clearConsoleButton;
    [SerializeField] private Button generateLogFileButton;
    public static string logFolder { get; private set; }
    private const string folderName = "DebugLogs";

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
        logFolder = $"{Path.GetFullPath("./")}{folderName}";
    }

    public void InitializeErrorConsole()
    {

        clearConsoleButton.onClick.AddListener(ClearErrorConsole);
        generateLogFileButton.onClick.AddListener(CreateLogFile);
    }    

    public void ClearErrorConsole()
    {
        foreach (TextMeshProUGUI g in RuntimeMsg.errorObjects)
        {
            Destroy(g.gameObject);
        }
        RuntimeMsg.errorObjects.Clear();
    }

    public void CreateLogFile()
    {

        string res = $"----Debug Log File----\n\nCreated On {Environment.MachineName}\nLocal Machine Time: {DateTime.Now}\nUTC Time: {DateTime.UtcNow}\n\n";

        if (RuntimeMsg.errorObjects.Count > 0)
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

        RuntimeMsg.Info($"Generated log file at: {logFolder}");
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


        foreach (TextMeshProUGUI t in RuntimeMsg.errorObjects)
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
}