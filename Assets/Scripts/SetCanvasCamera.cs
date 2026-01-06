using UnityEngine;

public class SetCanvasCamera : MonoBehaviour
{
    [SerializeField] private new Camera camera;
    [SerializeField] private cameraCanvasEnum chooseCamera;

    private void Start()
    {
        Canvas currentCanvas = GetComponent<Canvas>();
        if (camera == null) { 
            switch (chooseCamera)
            {
                case cameraCanvasEnum.MAINCAM:
                    camera = Camera.main;
                    break;
                case cameraCanvasEnum.CARDSCAM:
                    camera = GameObject.FindWithTag("CardsCam").GetComponent<Camera>();
                    break;
                case cameraCanvasEnum.UICAM:
                    camera = GameObject.FindWithTag("UICam").GetComponent<Camera>();
                    break;
                case cameraCanvasEnum.INFRONTOFUI:
                    camera = GameObject.FindWithTag("InFrontOfUI").GetComponent<Camera>();
                    break;
                    
                }
        }
        currentCanvas.worldCamera = camera;
    }
}

public enum cameraCanvasEnum
{
    MAINCAM,
    UICAM,
    CARDSCAM,
    INFRONTOFUI
}
