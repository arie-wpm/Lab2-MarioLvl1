using System;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Canvas mainCanvas;

    [SerializeField]
    private GameObject topPanel;

    [SerializeField]
    private GameObject blackPanel;

    [SerializeField]
    private GameObject livesPanel;

    [SerializeField] private GameObject ScorePopupPrefab;
    [SerializeField] private GameObject oneUpPopupPrefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Start() { }

    // Update is called once per frame
    void Update() { }

    public void DisableLivesScreen()
    {
        blackPanel.SetActive(false);
        livesPanel.SetActive(false);
    }

    public void EnableLivesScreen()
    {
        blackPanel.SetActive(true);
        livesPanel.SetActive(true);
    }

    public void SpawnPopup(int score, Vector3 spawnPos)
    {
        GameObject scorePopup = Instantiate(ScorePopupPrefab, mainCanvas.transform);
        
        RectTransform canvasRect = mainCanvas.GetComponent<RectTransform>();
        Vector3 screenPos = Camera.main.WorldToScreenPoint(spawnPos);
        
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, mainCanvas.worldCamera, out localPoint);
        
        scorePopup.GetComponent<RectTransform>().localPosition = localPoint;
        
        scorePopup.GetComponent<ScorePopupHandler>().ShowPopup(score);
    }
    
    public void Spawn1UPPopup(int score, Vector3 spawnPos)
    {
        GameObject oneUpPopup = Instantiate(oneUpPopupPrefab, mainCanvas.transform);
        
        RectTransform canvasRect = mainCanvas.GetComponent<RectTransform>();
        Vector3 screenPos = Camera.main.WorldToScreenPoint(spawnPos);
        
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, mainCanvas.worldCamera, out localPoint);
        
        oneUpPopup.GetComponent<RectTransform>().localPosition = localPoint;
    }
        
        
}
