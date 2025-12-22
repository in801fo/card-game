using UnityEngine;

public class SetCanvasCamera : MonoBehaviour
{
    [SerializeField] private Camera camera;

    private void Start()
    {
        Canvas currentCanvas = GetComponent<Canvas>();
        if (camera == null) { currentCanvas.worldCamera = Camera.main; return; }
        currentCanvas.worldCamera = camera;
    }
}
