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
}
