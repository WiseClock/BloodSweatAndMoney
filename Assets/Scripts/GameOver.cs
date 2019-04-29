using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    private Text _text;
    
    void Start()
    {
        float dollar = GameLoop.MoneyPower < 50 ? 50 / 800f : 1f + (GameLoop.MoneyPower - 50) * (GameLoop.MoneyPower - 50) / 10f;
        _text = GameObject.Find("Text").GetComponent<Text>();
        _text.text = $"GAME OVER, PRESIDENT.\n\nYour largest population is {GameLoop.LargestPopulation}, " +
                     $"and you once had {GameLoop.LargestWorkerCount} free workers at the same time!\n" +
                     $"Surely this includes {GameLoop.Captured} captured visitors and maybe {GameLoop.Killed} killed defectors.\n\n" +
                     $"But hey!  Your 1 dollar now equals to {dollar:0.###} American dollars!";
    }

    void Update()
    {
        
    }
}
