using System.IO;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

public class Load : MonoBehaviour
{
    private object mainSerializedObject;

    private string loadPath;

    private string folderPath = $"C:{Path.DirectorySeparatorChar}Users{Path.DirectorySeparatorChar}{Environment.UserName}{Path.DirectorySeparatorChar}Desktop{Path.DirectorySeparatorChar}DebugSaveFiles";

    private string defaultFileName = "DebugConfigFile";

    private List<FieldInfo> fields = new List<FieldInfo>();

    private const char subFieldDelimiter = '|';
    private const char generalFieldStart = ':';
    private const char lineEnd = '.';


    private void Start(){

        mainSerializedObject = DebugUIManager.serializingObject;
        //{defaultFileName}s is the name of the folder
        folderPath = $"{Path.GetFullPath("./")}{defaultFileName}s";
        loadPath = $"{folderPath}{Path.DirectorySeparatorChar}{defaultFileName}.txt";
        print(loadPath);

    }

    public void LoadFile(){
        if (!Directory.Exists(folderPath)){
            RuntimeMsg.Error(new DirectoryNotFoundException($"Directory {folderPath} not found, you either didn't save before loading (moron), or you simply deleted it (still a moron)"));
            return;
        }

        if (!File.Exists(loadPath)){
            RuntimeMsg.Error(new FileNotFoundException($"File {loadPath} not found, you either didn't save before loading (moron), or you simply deleted it (still a moron)"));
            return;
        }

        string[] fileLines = File.ReadAllLines(loadPath);

        if (fileLines.Length == 0){
            RuntimeMsg.Warning($"Tried to load {loadPath}, but the file was empty.");
            return;
        }

        for (int i = 0; i < fileLines.Length; i++)
        {
            if (string.IsNullOrEmpty(fileLines[i]) || string.IsNullOrWhiteSpace(fileLines[i])) continue;
            //            print("line:" + fileLines[i]);
            int fieldStarterPos = fileLines[i].IndexOf(generalFieldStart);
            if (fieldStarterPos == -1){
                RuntimeMsg.Warning("Unable to parse value", $"Unable to parse value {fileLines[i]}, skipping...");
                continue;
            }
            string varName = fileLines[i].Substring(0, fileLines[i].IndexOf(generalFieldStart));
            string value = fileLines[i].Substring(fileLines[i].IndexOf(generalFieldStart) + 1); 
            SetValues(varName, value);
        }

        DebugUIManager.drawer.RefreshUI();
    }

    private void SetValues(string varName, string value)
    {
        if (fields.Count == 0) fields = DebugUIManager.mainObjectFields;
        int fieldIndex = fields.FindIndex(info => info.Name.Contains(varName));
        if (fieldIndex == -1) return;

        FieldInfo varInfo = fields[fieldIndex];

        //a dictionary where:
        //Key: subFieldName
        //Value: subFieldValue
        Dictionary<string, string> subFieldsValues = new Dictionary<string, string>();

        //subFieldDelimiter characters are used as separators between subfields belonging to complex types.
        //so the presence of even one of these chars is an indicator of the current field being of a complex type
        int subFields = value.Count(ch => ch.Equals(subFieldDelimiter));
        if (subFields > 0)
        {

            subFieldsValues = HandleComplexField(value);
            foreach (var item in subFieldsValues)
            {
                DebugUIManager.changer.UpdateValue<string>($"{varInfo.Name}.{item.Key}", item.Value);
            }
        }
        else
        { //if no subFieldDelimiter character is found then we're handling a simple type
            value = value.Remove(value.LastIndexOf(lineEnd), 1);
            //            print("varName: " + varInfo.Name);
            DebugUIManager.changer.UpdateValue<string>(varInfo.Name, value);
        }

    }

    /// <summary>
    /// Splits the string version of a complex field in its core parts: names and values
    /// </summary>
    /// <param name="value">The complex field containing both names and values of the subfields</param>
    /// <returns>A dictionary as key the names of the subfields and as value th evalue of each subfield</returns>
    private Dictionary<string, string> HandleComplexField(string value){
        int subFields = value.Count(ch => ch.Equals(subFieldDelimiter));
        int nextFieldStart = 0;
        Dictionary<string, string> res = new Dictionary<string, string>();
        for (int i = 0; i < subFields; i++){
            string subFieldName = value.Substring(nextFieldStart, value.IndexOf(generalFieldStart, nextFieldStart) - nextFieldStart);
//            RuntimeError.Error("subFieldName: " + subFieldName);

            string subFieldValue = value.Substring(value.IndexOf(generalFieldStart, nextFieldStart)+1, value.IndexOf(subFieldDelimiter, nextFieldStart) - (value.IndexOf(generalFieldStart, nextFieldStart)+1));
//            RuntimeError.Error("subFieldValue: " + "\"" + subFieldValue + "\"");

            if (subFieldValue.Contains(lineEnd)) subFieldValue = subFieldValue.Remove(subFieldValue.IndexOf(lineEnd), 1);
            res.Add(subFieldName, subFieldValue);
            nextFieldStart = value.IndexOf(subFieldDelimiter, nextFieldStart) + 1;
        }

        return res;
    }

}
