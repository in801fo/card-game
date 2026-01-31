using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The base class from which every screen in the game inherits from
/// </summary>
/// <typeparam name="T">The type of data required from <c>Initialize</c> to correctly set all values</typeparam>
public class ScreenInitializer<T> : MonoBehaviour
{
    /// <summary>
    /// The local screen's id
    /// </summary>
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
        if (closeButton != null) closeButton.onClick.AddListener(() => ScreensManager.CloseScreen(_screenID));
        else RuntimeMsg.Warning($"No close button instance was provided for screen {this.name}");
        ScreensManager.OnScreenClosure += HandleClosureCheck;

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

    /// <summary>
    /// Override this method if you want to do stuff before the screen closure
    /// </summary>
    protected virtual void HandleClosure()
    {
        Destroy(gameObject);
    }

    protected void HandleClosureCheck(string id)
    {
        if (id != _screenID) return;
        HandleClosure();
    }

    protected void OnDestroy()
    {
        ScreensManager.OnScreenClosure -= HandleClosureCheck;
    }
}