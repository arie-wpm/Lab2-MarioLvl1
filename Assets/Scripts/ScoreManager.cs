using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class ScoreManager : MonoBehaviour
{
    //Static Variable that can be edited easily from any script.
    public static int Score;

    //Usable for nonstatic functions that will modify the score, however I doubt this will be needed.
    public int score
    {
        get { return Score; }
        set { score += value; }
    }

    public static void ModifyScore(int mod)
    {
        Score += mod;
    }

    public static int GetScore()
    {
        return Score;
    }
}
