using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGate : MonoBehaviour
{
    public int maxPlayersNeeded = 4;
    int playerCount = 0;
    [SerializeField] GameObject LevelCompleteScreen;
    [SerializeField] GameObject Player1;
    [SerializeField] GameObject Player2;
    [SerializeField] GameObject Player3;
    [SerializeField] GameObject Player4;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerCount++;
            if(playerCount == maxPlayersNeeded)
            {
                LevelCompleteScreen.SetActive(true);
                Player1.SetActive(false);
                Player2.SetActive(false);
                Player3.SetActive(false);
                Player4.SetActive(false);
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerCount--;
            print(playerCount);
        }
    }
}
