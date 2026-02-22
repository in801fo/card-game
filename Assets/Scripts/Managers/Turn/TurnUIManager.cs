using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TurnUIManager : MonoBehaviour
{
    [SerializeField] private Button finishTurnButtonPrefab;
    private Button finishButtonInstance;

    private Canvas turnUICanvas;

    private void Awake()
    {
        CreateUICanvas();

        finishButtonInstance = Instantiate(finishTurnButtonPrefab);

        RectTransform buttonRT = finishButtonInstance.GetComponent<RectTransform>();

        buttonRT.position = Camera.main.ViewportToWorldPoint(Vector3.one * -1) + new Vector3(buttonRT.rect.width / 1.5f, buttonRT.rect.height);
        finishButtonInstance.transform.SetParent(turnUICanvas.transform);


        finishButtonInstance.onClick.AddListener(HandleSkipTurn);
        TurnManager.OnLocalTurnStart += HandleTurnStart;
        TurnManager.OnLocalTurnOver += HandleTurnOver;
    }

    private void CreateUICanvas()
    {
        turnUICanvas = new GameObject("TurnUICanvas").AddComponent<Canvas>();
        turnUICanvas.AddComponent<GraphicRaycaster>();
        turnUICanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = turnUICanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        scaler.referenceResolution = new Vector2(1920, 1080);
    }

    private void HandleSkipTurn()
    {
        TurnManager.Instance.HandleSkipTurn();
    }

    private void HandleTurnOver()
    {
        finishButtonInstance.gameObject.SetActive(false);
    }

    private void HandleTurnStart()
    {
        finishButtonInstance.gameObject.SetActive(true);
    }
}