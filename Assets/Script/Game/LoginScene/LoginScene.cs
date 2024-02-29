using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginScene : MonoBehaviour
{
    public TMP_InputField unameInput;
    public TMP_InputField upwdInput;

    private void OnLoginSuccess(string name, object udata)
    {
        SystemServiceProxy.Instance.GetUgameInfo();
        Debug.Log("OnLoginSuccess");
    }

    private void OnGetUgameInfo(string name, object udata)
    {
        LogicServiceProxy.Instance.LogicLogin();
        Debug.Log("OnGetUgameInfo");
    }

    private void OnLogicLoginSuccess(string name, object udata)
    {
        // Load game data
        SystemServiceProxy.Instance.GetUgameInfo();
        SceneManager.LoadScene("HomeScene");
        Debug.Log("OnLogicLoginSuccess");
    }

    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.AddEventListener("loginSuccess", OnLoginSuccess);
        EventManager.Instance.AddEventListener("getUgameInfo", OnGetUgameInfo);
        EventManager.Instance.AddEventListener("logicLoginSuccess",OnLogicLoginSuccess);
    }

    public void onGuestLoginClick()
    {
        AuthServiceProxy.Instance.GuestLogin();
    }

    void OnDestroy()
    {
        EventManager.Instance.RemoveEventListener("loginSuccess", OnLoginSuccess);
        EventManager.Instance.RemoveEventListener("getUgameInfo", OnGetUgameInfo);
        EventManager.Instance.RemoveEventListener("logicLoginSuccess", OnLogicLoginSuccess);
    }

    public void OnUserLoginClick()
    {
        if (this.unameInput.text.Length <= 0 || this.upwdInput.text.Length <= 0)
        {
            Debug.Log("Username or password is empty");
            return;
        }

        AuthServiceProxy.Instance.UserAccountLogin(unameInput.text, upwdInput.text);
    }
}
