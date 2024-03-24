using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// import TMPro
using TMPro;
using UnityEngine.SceneManagement;

public class HomeScene : MonoBehaviour
{
    // tmpro text unick
    public GameObject UInfoDlgPrefab;
    public TextMeshProUGUI unick;
    public Image avatar = null;
    public Sprite[] sysAvatars = null;

    // new region UGameInfo in inspector
    [Header("UGameInfo")]
    public TextMeshProUGUI ugold;
    public TextMeshProUGUI udiamond;
    public TextMeshProUGUI uexp;
    public TextMeshProUGUI uvip;

    [Header("Homepage")]
    public GameObject homepage;

    [Header("Warpage")]
    public GameObject warpage;
    public GameObject teamMatchPrefab;

    [Header("Loadpage")]
    public GameObject loadpage;


    private void OnSyncUInfo(string name, object udata)
    {
        if (this.unick != null)
        {
            unick.text = UGame.Instance.uNick;
        }

        if (this.avatar != null)
        {
            this.avatar.sprite = sysAvatars[UGame.Instance.uSystemAvatar % sysAvatars.Length];
        }
    }

    private void OnLogout(string name, object udata)
    {
        SceneManager.LoadScene("LoginScene");
    }

    private void OnSyncUGameInfo(string name, object udata)
    {
        if (ugold)
        {
            ugold.text = UGame.Instance.uGameInfo.Uchip.ToString();
        }
        if (udiamond)
        {
            udiamond.text = UGame.Instance.uGameInfo.Uchip2.ToString();
        }
        if (uexp)
        {
            uexp.text = UGame.Instance.uGameInfo.Uexp.ToString();
        }
        if (uvip)
        {
            uvip.text = UGame.Instance.uGameInfo.Uvip.ToString();
        }
    }

    public void OnHomePageBtnClose()
    {
        homepage.SetActive(false);
    }

    public void OnHomepageBtnClick()
    {
        homepage.SetActive(true);
        warpage.SetActive(false);
    }

    public void OnWarpageBtnClose()
    {
        warpage.SetActive(false);
    }

    public void OnWarpageBtnClick()
    {
        warpage.SetActive(true);
        homepage.SetActive(false);
    }

    public void OnTeamMatchButtonClick(int zoneId)
    {
        if (teamMatchPrefab != null)
        {
            GameObject dlg = Instantiate(teamMatchPrefab);
            dlg.transform.SetParent(this.transform, false);
            UGame.Instance.zoneid = zoneId;
        }
    }

    private void OnGameStart(string name, object udata)
    {
        //SceneManager.LoadScene("GameScene");
        this.loadpage.SetActive(true);
    }


    // Start is called before the first frame update
    void Start()
    {
        EventManager.Instance.AddEventListener("SyncUInfo", OnSyncUInfo);
        EventManager.Instance.AddEventListener("SyncUGameInfo", OnSyncUGameInfo);
        EventManager.Instance.AddEventListener("logout", OnLogout);
        EventManager.Instance.AddEventListener("gameStart", OnGameStart);

        OnSyncUInfo("SyncUInfo", null);
        OnSyncUGameInfo("SyncUGameInfo", null);
    }

    void OnDestroy()
    {
        EventManager.Instance.RemoveEventListener("SyncUInfo", OnSyncUInfo);
        EventManager.Instance.RemoveEventListener("SyncUGameInfo", OnSyncUGameInfo);
        EventManager.Instance.RemoveEventListener("logout", OnLogout);
        EventManager.Instance.RemoveEventListener("gameStart", OnGameStart);
    }

    public void OnUInfoClick()
    {
        if (UInfoDlgPrefab != null)
        {
            GameObject dlg = Instantiate(UInfoDlgPrefab);
            dlg.transform.SetParent(this.transform, false);
        }
    }

}
