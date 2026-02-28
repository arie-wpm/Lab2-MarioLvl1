using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScorePopupHandler : MonoBehaviour
{
    [SerializeField] private float movespeed;
    [SerializeField] private float despawnDelay;
    private float timer;
    
    [SerializeField] private GameObject[] hundredsArr;
    [SerializeField] private GameObject[] thousandsArr;

    private int thousands;
    private int hundreds;

    private static readonly Dictionary<int, int> digitToArr = new Dictionary<int, int>
    {
        { 0, 0 },
        { 1, 1 },
        { 2, 2 },
        { 4, 3 },
        { 5, 4 },
        { 8, 5 }
    };
    
    public void ShowPopup(int score)
    {
        ScoreToDigits(score);

        if (digitToArr.TryGetValue(thousands, out int i))
        {
            thousandsArr[i].SetActive(true);
        }
        if (digitToArr.TryGetValue(hundreds, out int j))
        {
            hundredsArr[j].SetActive(true);
        }
    }

    private void ScoreToDigits(int score)
    {
        thousands = score / 1000;
        hundreds = (score % 1000) / 100;
    }

    private void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;
        if (timer < despawnDelay)
        {
            gameObject.transform.Translate(Vector2.up * (movespeed * Time.fixedDeltaTime));
        }
        else
        {
            Destroy(gameObject);
        }
    }

}
