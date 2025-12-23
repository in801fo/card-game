using TMPro;
using UnityEngine;
using System.Reflection;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class DrawDebugUI : MonoBehaviour
{
    [SerializeField] private Canvas _debugCanvas;
    [SerializeField] private GameObject _debugView;
    [SerializeField] private GameObject _errorConsole;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject togglePrefab;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private GameObject _saveLoadInterface;

    public GameObject errorConsole { get;  private set;}

    /// <summary>
    /// Used to know if currently we are expanding a type that belongs to another serialized object.
    /// </summary>
    private bool isExpanding;
    private Transform componentAddParent;
    private Canvas parentCanvas;
    /// <summary>
    /// A reference of the currently reflecting object.
    /// </summary>
    private object mainSerializingObject;
    /// <summary>
    /// A variable used to store the FieldInfo about the currently expanding field.
    /// </summary>
    private FieldInfo currentSerializingFieldInfo;

    private GameObject debugView;
    private GameObject fieldDebugCollection;

    private List<GameObject> fieldContainers = new List<GameObject>();


    private void Awake(){
        InitializeUI();
    }
    

    private void Update(){
        if (Input.GetKeyDown(KeyCode.T)){
            fieldDebugCollection.SetActive(!fieldDebugCollection.activeInHierarchy);
        }

        if (Input.GetKeyDown(KeyCode.J)){
            errorConsole.SetActive(!errorConsole.activeInHierarchy);
        }
    }


    private void InitializeUI(){
        CreateDebugCanvas();
        CreateDebugScrollView();
        CreateSaveLoadInterface();
        CreateErrorConsole();

        fieldDebugCollection.SetActive(false);
        errorConsole.SetActive(false);
    }

    private void CreateDebugCanvas(){
        //debug Canvas
        parentCanvas = Instantiate(_debugCanvas, Vector3.zero, Quaternion.identity);
        fieldDebugCollection = new GameObject("Field Debug Collection");
        fieldDebugCollection.transform.SetParent(parentCanvas.transform);
        fieldDebugCollection.transform.localPosition = Vector3.zero;
    }

    private void CreateDebugScrollView(){
        //debug Scroll View
        debugView = Instantiate(_debugView, Vector3.zero, Quaternion.identity);
        debugView.transform.SetParent(fieldDebugCollection.transform);
        RectTransform debugViewRectTransform = debugView.GetComponent<RectTransform>();
        debugViewRectTransform.anchoredPosition = Vector3.zero;
        debugViewRectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);

        //save debug Scroll View transform 
        GameObject debugViewContent = debugView.GetComponentInChildren<VerticalLayoutGroup>().gameObject;
        componentAddParent = debugViewContent.transform;
    }

    private void CreateSaveLoadInterface(){
        //save-load interface
        GameObject saveLoadInterface = Instantiate(_saveLoadInterface).gameObject;
        //changing the parent two times because doing otherwhise (changing it only one time)
        //would result in the interface not positioning correctly 
        saveLoadInterface.transform.SetParent(parentCanvas.transform);
        saveLoadInterface.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        saveLoadInterface.transform.SetParent(fieldDebugCollection.transform);
    }

    private void CreateErrorConsole(){
        //error console
        errorConsole = Instantiate(_errorConsole, Vector3.zero, Quaternion.identity);
        errorConsole.transform.SetParent(parentCanvas.transform);
        RectTransform consoleTransform = errorConsole.GetComponentInChildren<RectTransform>();
        consoleTransform.anchoredPosition = Vector3.zero;
        consoleTransform.sizeDelta = new Vector2(Screen.width, consoleTransform.sizeDelta.y);
    }

    /// <summary>
    /// Generates a runtime debug UI for an object.
    /// </summary>
    /// <param name="value">The main object from which to generate the UI</param>
    /// <param name="allowRecursiveSerialization">Allows the serialization of fields of complex types withing the provided object</param>
    public void GenerateDebugUIForValue(object value, bool allowRecursiveSerialization)
    {
        if (!isExpanding) mainSerializingObject = value;

        //Get the fields
        List<FieldInfo> objectFields = DebugUIManager.mainObjectFields;
        if (isExpanding) objectFields = value.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy).ToList();

        for (int i = 0; i < objectFields.Count; i++){
            
            if (!DebugUIManager.HasToBeExpanded(objectFields[i].GetValue(value)) || isExpanding){
                string fieldName = isExpanding ? $"{currentSerializingFieldInfo.Name}.{objectFields[i].Name}" : objectFields[i].Name;
                GenerateLabelWithField(fieldName, objectFields[i].GetValue(value));
            }
            else if (allowRecursiveSerialization){
                currentSerializingFieldInfo = objectFields[i];
                RecursiveGenerateUI(objectFields[i].GetValue(value), true);
            }
        }
    }


    //separated the two because it wasn't worth the risk exposing the param "isExpanding" as an optional parameter
    private void RecursiveGenerateUI(object value, bool isExpanding){
        this.isExpanding = isExpanding;
        GenerateDebugUIForValue(value, false);
        this.isExpanding = false;
    }

    private void GenerateLabelWithField(string propertyName, object propertyValue){
        //create an empty parent, just to keep things ordered and to simplify the process of ordering the objects in the UI
        RectTransform propertyParent = GeneratePropertyParent($"{propertyName}", componentAddParent);
        fieldContainers.Add(propertyParent.gameObject);
        string label = propertyName;
        if (propertyName.Contains(".")) label = propertyName.Remove(0, propertyName.LastIndexOf(".") + 1);

        //instantiate a label for the fields
        TextMeshProUGUI propertyLabel = CreateDefaultText($"{label} Label", componentAddParent);
        //if we are currently expanding, set the name to: "theNameOfTheField: {the name of the field belonging to the currently expanding field}"
        //otherwhise, set it to the name of the field
        propertyLabel.text = isExpanding ? $"{currentSerializingFieldInfo.Name}: {label}" : label;
        propertyLabel.transform.SetParent(propertyParent);

        GameObject inputField = GenerateInputField(propertyValue);

        if(propertyValue == null){
            if (inputField.TryGetComponent(out TMP_InputField textField)) textField.text = "<null>";
            if (inputField.TryGetComponent(out Toggle toggle)) toggle.isOn = false;
        }else{
            inputField.GetComponent<ValueUpdater>().varName = propertyName;
            
            if (propertyValue.GetType() == typeof(bool)){
                inputField.GetComponent<Toggle>().isOn = (bool)propertyValue;
            }else{
                TMP_InputField inputFieldComponent = inputField.GetComponent<TMP_InputField>();
                
                inputFieldComponent.text = propertyValue.ToString();
            }
        }
        inputField.transform.SetParent(propertyParent);
    }

    private RectTransform GeneratePropertyParent(string propertyName, Transform parent){

        GameObject propertyParentGo = new GameObject($"{propertyName} group");
        RectTransform propertyParent = propertyParentGo.AddComponent<RectTransform>();
        HorizontalLayoutGroup layoutGroup = propertyParentGo.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.childForceExpandHeight = false;
        propertyParent.transform.SetParent(parent);

        return propertyParent;
    }

    public TextMeshProUGUI CreateDefaultText(string name, Transform parent){
        TextMeshProUGUI defaultText = Instantiate(text);
        defaultText.name = name;
        defaultText.transform.SetParent(parent);

        return defaultText;
    }

    public GameObject GenerateInputField(object value){
        
        if(value != null && value.GetType() == typeof(bool)) return Instantiate(togglePrefab, Vector3.zero, Quaternion.identity);
        return Instantiate(inputField.gameObject, Vector3.zero, Quaternion.identity);
    }

    private void DestroyOutdatedUI(){
        foreach(GameObject field in fieldContainers){
            Destroy(field);
        }
    }

    public void RefreshUI(){
        DestroyOutdatedUI();
        GenerateDebugUIForValue(mainSerializingObject, true);
    }

}
