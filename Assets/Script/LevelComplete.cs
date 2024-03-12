using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelComplete : MonoBehaviour
{
    public int LastLevelVal = 0;
    public int NextLevelVal = 1;

    public void LastLevel()
    {
        SceneManager.LoadScene(LastLevelVal);
    }

    public void NextLevel()
    {
        SceneManager.LoadScene(NextLevelVal);
    }
}
