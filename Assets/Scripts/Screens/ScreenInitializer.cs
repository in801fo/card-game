using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class ScreenInitializer<T> : MonoBehaviour
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

    [field: SerializeField] public Button closeButton { get; protected set; }

    protected bool hasInitialized;

    private void Awake()
    {
        if (closeButton != null) closeButton.onClick.AddListener(() => GameScreensManager.CloseCurrentScreen());
    }

    public virtual void Initialize(T initializingValues)
    {
        hasInitialized = true;   
    }
}