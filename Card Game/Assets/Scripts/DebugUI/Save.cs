using System.IO;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;
using System.Text;

public class Save : MonoBehaviour
{
    [HideInInspector] public string text = "Decompile My Balls, Bitch";
    private const string defaultFileName = "DebugConfigFile";

    private string debugDirectoryPath;

    private object mainSerializingObject;

    private FieldInfo currentSerializingObject;

    private List<FieldInfo> actualInfo = new List<FieldInfo>();

    private string saveFilePath;

    private string saveFile;



    private void Start(){
        saveFilePath = Path.GetFullPath("./");
        //if you can't find the folder on the desktop, check on the OneDrive folder
        //and iof you find it there, from now on, save there
        /*if (!Directory.Exists(saveFilePath) && Directory.Exists($"C:{Path.DirectorySeparatorChar}Users{Path.DirectorySeparatorChar}{Environment.UserName}{Path.DirectorySeparatorChar}OneDrive"))
            saveFilePath = $"C:{Path.DirectorySeparatorChar}Users{Path.DirectorySeparatorChar}{Environment.UserName}{Path.DirectorySeparatorChar}OneDrive{Path.DirectorySeparatorChar}Desktop";
        */

        debugDirectoryPath = $"{saveFilePath}{Path.DirectorySeparatorChar}{defaultFileName}s";
        saveFile = $"{debugDirectoryPath}{Path.DirectorySeparatorChar}{defaultFileName}.txt";
        
        if(!Directory.Exists(debugDirectoryPath)) Directory.CreateDirectory(debugDirectoryPath);
        if (!File.Exists(saveFile)) HandleCreateFile(); 
        
    }

    private void HandleCreateFile()
    {
        using (FileStream stream = File.Create(saveFile))
        {
            stream.Dispose();
            stream.Close();
        }
    }

    public void SaveSettings()
    {
        mainSerializingObject = DebugUIManager.serializingObject;
        if (actualInfo.Count == 0) actualInfo = DebugUIManager.mainObjectFields;

        using (FileStream stream = File.Open(saveFile, FileMode.OpenOrCreate))
        {

            for (int i = 0; i < actualInfo.Count; i++)
            {
                string fieldName = actualInfo[i].Name;
                object fieldValue = actualInfo[i].GetValue(mainSerializingObject);
                //print("value: " + fieldValue);

                string res;
                byte[] arr;
                if (!DebugUIManager.HasToBeExpanded(fieldValue)) res = $"{FormatInfo(fieldName, fieldValue)}.\n";
                else
                {
                    currentSerializingObject = actualInfo[i];
                    fieldName = currentSerializingObject.Name;
                    res = ExpandInfo(fieldName, fieldValue);
                }

                arr = Encoding.ASCII.GetBytes(res);
                stream.Write(arr, 0, arr.Length);
            }

            stream.Flush();
            stream.Dispose();
        }
    }

    private string FormatInfo(string fieldName, object propertyValue){
        if (propertyValue.ToString().Contains('.')) propertyValue = propertyValue.ToString().Replace('.', ',');
        return $"{fieldName}: {propertyValue}";
    }

    private string ExpandInfo(string fieldName, object value){
        string res = $"{fieldName}:";
        FieldInfo[] fields = value.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

        foreach(FieldInfo info in fields){
            string fName = info.Name;
            object fValue = info.GetValue(currentSerializingObject.GetValue(mainSerializingObject));
            res += $"{FormatInfo(fName, fValue)}|";
        }

        res += ".\n";

        return res;
    }

}
