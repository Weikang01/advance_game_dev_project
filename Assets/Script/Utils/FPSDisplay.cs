using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    private float timeDelta = 0.5f;
    private float prevTime = 0.0f;
    private float fps = 0.0f;
    private int accFrame = 0;  // accumulated frame

    private GUIStyle style = new GUIStyle();


    private void Awake()
    {
        Application.targetFrameRate = 60;
    }


    void OnGUI()
    {
        GUI.Label(new Rect(10, Screen.height - 20, 100, 20), "FPS: " + this.fps.ToString("f2"), this.style);
    }

    // Start is called before the first frame update
    void Start()
    {
        this.prevTime = Time.realtimeSinceStartup;
        this.style.fontSize = 20;
        this.style.normal.textColor = Color.white;
    }

    // Update is called once per frame
    void Update()
    {
        this.accFrame++;
        if (Time.realtimeSinceStartup - this.prevTime >= this.timeDelta)
        {
            this.fps = this.accFrame / (Time.realtimeSinceStartup - this.prevTime);
            this.accFrame = 0;
            this.prevTime = Time.realtimeSinceStartup;
        }
    }
}
