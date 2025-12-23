using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class DebugUIManager : MonoBehaviour
{
    [SerializeField] private DrawDebugUI _drawer;
    [SerializeField] private UpdateDebugUI _changer;
    [SerializeField] private RuntimeError _runtimeErrorDisplay;

    public static object serializingObject { get; private set; }
    public static List<FieldInfo> mainObjectFields = new List<FieldInfo>();

    public static DrawDebugUI drawer {get;  private set;}
    public static UpdateDebugUI changer {get;  private set;}
    //could make an enum 
    private static List<string> _defaultTypes = new List<string>(new string[] { "System.Single", "System.Int32", "System.String", "System.Boolean" });
    public static List<string> defaultTypes { 
        get { return _defaultTypes; }
        private set{}
    } 

    private void Awake(){
        drawer = Instantiate(_drawer.gameObject).GetComponent<DrawDebugUI>();
        changer = Instantiate(_changer.gameObject).GetComponent<UpdateDebugUI>();
        Instantiate(_runtimeErrorDisplay.gameObject).GetComponent<RuntimeError>();
        if (!EventSystem.current) CreateEventSystem();
    }

    private void CreateEventSystem()
    {
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }

    public static void GenerateUIForValue(object value, bool serializeRecursively, bool isReadOnly){
        serializingObject = value;
        mainObjectFields = serializingObject.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy).ToList();
        drawer.GenerateDebugUIForValue(value, serializeRecursively);
        if (!isReadOnly) changer.StartVariableUpdate(value);
    }

    public static bool HasToBeExpanded(object propertyValue){
        if (propertyValue == null) return false;
        return !defaultTypes.Contains(propertyValue.GetType().ToString());
    }

}
