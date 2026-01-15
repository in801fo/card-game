using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScreenInitializer<T> : MonoBehaviour
{
    public string screenID
    {
        get
        {
            return _screenID;
        } 
        set
        {
            //set it only once
            if (_screenID == null || _screenID == string.Empty) 
                _screenID = value;
        }
    }

    protected string _screenID;

    [field: SerializeField] protected Button closeButton { get; set; }
    [field: SerializeField] protected TextMeshProUGUI screenHeading { get; set; }

    [SerializeField] protected string screenHeadingText;

    protected bool hasInitialized;

    protected virtual void Awake()
    {
        if (closeButton != null) closeButton.onClick.AddListener(GameScreensManager.CloseCurrentScreen);
        else RuntimeMsg.Warning($"No close button instance was provided for screen {this.name}");

        SetScreenHeading(string.Empty, false);
    }

    protected void SetScreenHeading(string str = "", bool overrideEditorHeading = true)
    {
        if (overrideEditorHeading) screenHeadingText = str;

        if (screenHeading != null) screenHeading.SetText(screenHeadingText);
        else RuntimeMsg.Warning($"No screen heading instance was provided for screen {this.name}");
    
    }

    /// <summary>
    /// Place the base of the method at the end of the overridden versions
    /// </summary>
    /// <param name="initializingValues"></param>
    public virtual void Initialize(T initializingValues)
    {
        hasInitialized = true;   
    }
}