using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AsyncSceneLoader : MonoBehaviour
{
    public string sceneName; // Name of the scene to load
    public Image progressBar;  // Reference to the UI progress bar

    private AsyncOperation ao;  // Asynchronous operation

    private void OnEnable()
    {
        StartCoroutine(AsyncLoad());
    }

    IEnumerator AsyncLoad()
    {
        this.ao = SceneManager.LoadSceneAsync(sceneName);
        this.ao.allowSceneActivation = false;

        yield return this.ao;
    }

    private void Update()
    {
        if (ao != null)
        {
            float per = this.ao.progress;  // percentage value (max 0.9)
            progressBar.fillAmount = per * 1.111f;
            if (per >= 0.9f)
            {
                this.ao.allowSceneActivation = true;
            }
        }
    }
}
