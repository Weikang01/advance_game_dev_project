using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamMatch : MonoBehaviour
{
    public GameObject MatchItemPrefab;
    public ScrollRect MatchItemHolder;
    public Sprite[] sysavatars = null;

    private void OnEnterMatchSuccess(string name, object udata)
    {
        EnterMatch enterMatch = (EnterMatch)udata;
        if (enterMatch.OtherUinfo.Count > 0 && MatchItemPrefab)
        {
            foreach (var uinfo in enterMatch.OtherUinfo)
            {
                GameObject matchItem = Instantiate(MatchItemPrefab);
                matchItem.transform.SetParent(MatchItemHolder.content, false);
                MatchItemHolder.content.sizeDelta = new Vector2(MatchItemHolder.content.sizeDelta.x, MatchItemHolder.content.sizeDelta.y + 106);
                matchItem.transform.Find("name").GetComponent<Text>().text = uinfo.Unick;
                matchItem.transform.Find("header/avator").GetComponent<Image>().sprite = sysavatars[uinfo.Usysavatar % sysavatars.Length];
                matchItem.transform.Find("sex").GetComponent<Text>().text = uinfo.Usex == 1 ? "Male" : (uinfo.Usex == 2? "Female" : "Unknown");
            }
        }
    }

    private void OnOtherEnteredMatch(string name, object udata)
    {
        OnOtherEnteredMatch uinfo = (OnOtherEnteredMatch)udata;
        if (MatchItemPrefab)
        {
            GameObject matchItem = Instantiate(MatchItemPrefab);
            matchItem.transform.SetParent(MatchItemHolder.content, false);
            MatchItemHolder.content.sizeDelta = new Vector2(MatchItemHolder.content.sizeDelta.x, MatchItemHolder.content.sizeDelta.y + 106);
            matchItem.transform.Find("name").GetComponent<Text>().text = uinfo.Unick;
            matchItem.transform.Find("header/avator").GetComponent<Image>().sprite = sysavatars[uinfo.Usysavatar % sysavatars.Length];
            matchItem.transform.Find("sex").GetComponent<Text>().text = uinfo.Usex == 1 ? "Male" : (uinfo.Usex == 2 ? "Female" : "Unknown");
        }
    }

    private void OnOtherQuittedMatch(string name, object udata)
    {
        int index =(int)udata;
        Debug.Log("Removing item " + index);

        GameObject.Destroy(MatchItemHolder.content.GetChild(index).gameObject);
        MatchItemHolder.content.sizeDelta = new Vector2(MatchItemHolder.content.sizeDelta.x, MatchItemHolder.content.sizeDelta.y - 106);
    }

    private void OnQuitMatchSuccess(string name, object udata)
    {
        // delete all children
        UGame.Instance.zoneid = -1;
        GameObject.Destroy(this.gameObject);
    }

    private void OnGameStart(string name, object udata)
    {
        GameObject.Destroy(this.gameObject);
    }

    // Start is called before the first frame update
    private void OnEnable()
    {
        EventManager.Instance.AddEventListener("enterMatchSuccess", OnEnterMatchSuccess);
        EventManager.Instance.AddEventListener("otherEnteredMatch", OnOtherEnteredMatch);
        EventManager.Instance.AddEventListener("quitMatchSuccess", OnQuitMatchSuccess);
        EventManager.Instance.AddEventListener("otherQuittedMatch", OnOtherQuittedMatch);
        EventManager.Instance.AddEventListener("gameStart", OnGameStart);
    }

    public void OnBeginMatchClick()
    {
        LogicServiceProxy.Instance.EnterZone(UGame.Instance.zoneid);
    }

    public void OnQuitBtnClick()
    {
        LogicServiceProxy.Instance.QuitMatch();
    }

    public void OnCloseBtnClick()
    {
        LogicServiceProxy.Instance.QuitMatch();
        UGame.Instance.zoneid = -1;
        GameObject.Destroy(this.gameObject);
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveEventListener("enterMatchSuccess", OnEnterMatchSuccess);
        EventManager.Instance.RemoveEventListener("otherEnteredMatch", OnOtherEnteredMatch);
        EventManager.Instance.RemoveEventListener("quitMatchSuccess", OnQuitMatchSuccess);
        EventManager.Instance.RemoveEventListener("otherQuittedMatch", OnOtherQuittedMatch);
        EventManager.Instance.RemoveEventListener("gameStart", OnGameStart);
    }
}
