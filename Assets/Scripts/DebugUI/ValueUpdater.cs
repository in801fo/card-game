using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ValueUpdater : MonoBehaviour
{

    public string varName { get;  set;}

    private Toggle toggle;

    private TMP_InputField inputField;

    private bool isToggle;

    private void Start(){
        if(TryGetComponent<Toggle>(out Toggle toggle)){
            this.toggle = toggle;
            isToggle = true;
        }else{
            inputField = this.GetComponent<TMP_InputField>();
        }

        if (isToggle) toggle.onValueChanged.AddListener(HandleToggle);
        else inputField.onValueChanged.AddListener(HandleInputField);
    }

    private void HandleToggle(bool value){
        UpdateDebugUI.Instance.UpdateValue(varName, value);
    }

    
    private void HandleInputField(string value){
        UpdateDebugUI.Instance.UpdateValue(varName, value);
    }

    
}
