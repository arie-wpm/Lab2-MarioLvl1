using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    //Static Variable that can be edited easily from any script.
    public static int Score;
    public static int Combo = 0;
    public static float ScoreMultiplier;
    [SerializeField] private static float comboResetTime = 0.8f; // time in seconds before combo resets
    private static float lastScoreTime;
    
    [SerializeField] private UIManager uiManager;

    public static UnityEvent ScoreChanged;

    void Awake()
    {
        Instance = this;
        ScoreChanged ??= new UnityEvent();
    }

    //Usable for nonstatic functions that will modify the score, however I doubt this will be needed.
    public int score
    {
        get { return Score; }
        set { score += value; }
    }

    public static void AddScoreWithModifier(int points, Vector3 spawnPos) //for enemies - spawnPos for popup position
    {
        float timeSinceLastScore = Time.time - lastScoreTime;
        if (timeSinceLastScore <= comboResetTime)
        {
            Debug.Log(timeSinceLastScore);
            Combo++;
        }
        else
        {
            Combo = 0;
        }
        lastScoreTime = Time.time;
        
        ScoreMultiplier = Mathf.Pow(2, Combo);
        points *= (int)ScoreMultiplier;
        Score += points;
        
        Instance.uiManager.SpawnPopup(points, spawnPos);
    }
    
    public static void ModifyScore(int mod)
    {
        Score += mod;
    }
    
    public static void AddScore(int points) //no modifiers - for pickups etc
    {
        Score += points;
    }

    public static int GetScore()
    {
        return Score;
    }

    public static void ResetScore()
    {
        Score = 0;
    }
}
