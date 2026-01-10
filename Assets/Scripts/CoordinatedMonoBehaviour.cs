using UnityEngine;

/// <summary>
/// A class that responds to the OnDoneGenerating action of the GameManager
/// Implemented ot avoid coordination problems when instantiating the managers 
/// at the beginning of runtime 
/// </summary>
public abstract class CoordinatedMonoBehaviour : MonoBehaviour
{
    private bool Ready;

    /// <summary>
    /// When using Awake, please keep the base at the top of the overridden version otherwise nothing will start
    /// </summary>
    protected virtual void Awake()
    {
        GameManager.OnDoneGenerating += HandleGenerationDone;
        GameManager.ValidateInitialization();
    }

    /// <summary>
    /// Update method which starts updating only when the generation is done
    /// </summary>
    protected virtual void ReadyUpdate(){}

    /// <summary>
    /// Implement here all that is needed when the generation is done, such as references to other managers/scripts
    /// which are instantiated at the start of a scene
    /// </summary>
    protected abstract void Beginning();

    protected virtual void Update()
    {
        if (Ready) ReadyUpdate();
    }

    private void HandleGenerationDone()
    {
        Beginning();
        Ready = true;
    }

    private void OnDestroy()
    {
        GameManager.OnDoneGenerating -= Beginning;
    }
}
