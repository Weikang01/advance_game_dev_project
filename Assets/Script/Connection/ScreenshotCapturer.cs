using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenshotCapturer : MonoBehaviour
{
    public short width = 256;
    public short height = 256;
    public bool continuous = true;

    private GameSocket gameSocket;
    private bool capturing = false;

    public Image testImage;

    // Start is called before the first frame update
    void Start()
    {
        gameSocket = GameSocket.GetInstance();
    }

    void Update()
    {
        if (continuous && !capturing)
        {
            StartCoroutine(CaptureScreenshot());
        }
    }

    Texture2D Resize(Texture2D texture2D, int targetX, int targetY)
    {
        RenderTexture rt = new RenderTexture(targetX, targetY, 24);
        RenderTexture.active = rt;
        Graphics.Blit(texture2D, rt);
        Texture2D result = new Texture2D(targetX, targetY);
        result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
        result.Apply();
        return result;
    }

    private IEnumerator CaptureScreenshot()
    {
        capturing = true;

        // Wait for the end of the current frame
        yield return new WaitForEndOfFrame();

        // Now it's safe to capture the screenshot
        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();

        screenshot = Resize(screenshot, width, height);

        if (testImage != null)
        {
            testImage.sprite = Sprite.Create(screenshot, new Rect(0, 0, screenshot.width, screenshot.height), new Vector2(0, 0));
        }

        if (screenshot == null)
        {
            Debug.Log("Failed to capture screenshot");
            //capturing = false;
            yield break;
        }
        byte[] ss = screenshot.EncodeToPNG();

        gameSocket.SendScreenShot(0, width, height, ss);
        Debug.Log("ss.Length: " + ss.Length);

        capturing = false;
    }
}
