using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGate : MonoBehaviour
{
    public int maxPlayersNeeded = 4;
    int playerCount = 0;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerCount++;
            print(playerCount);
            if(playerCount == maxPlayersNeeded)
            {
                SceneManager.LoadScene("TileGenerationTestGround");
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
