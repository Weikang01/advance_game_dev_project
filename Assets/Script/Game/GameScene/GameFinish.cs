using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameFinish : MonoBehaviour
{
    public GameObject PlayerCardPrefab;
    public ScrollRect PlayerCardHolder;
    public Sprite[] sysavatars = null;

    // Start is called before the first frame update
    void OnEnable()
    {
        EventManager.Instance.AddEventListener("quitMatchSuccess", OnQuitMatchSuccess);

    }

    private void OnQuitMatchSuccess(string name, object udata)
    {
        SceneManager.LoadScene("HomeScene");
    }

    public void OnGameFinished(int WinnerTeamid)
    {
        if (PlayerCardPrefab)
        {
            if (UGame.Instance.self_teamid == WinnerTeamid)
            {
                GameObject matchItem = Instantiate(PlayerCardPrefab);
                matchItem.transform.SetParent(PlayerCardHolder.content, false);
                PlayerCardHolder.content.sizeDelta = new Vector2(PlayerCardHolder.content.sizeDelta.x, PlayerCardHolder.content.sizeDelta.y + 106);
                matchItem.transform.Find("name").GetComponent<Text>().text = UGame.Instance.uNick;
                matchItem.transform.Find("header/avator").GetComponent<Image>().sprite = sysavatars[UGame.Instance.uSystemAvatar % sysavatars.Length];
                matchItem.transform.Find("sex").GetComponent<Text>().text = UGame.Instance.uSex == 1 ? "Male" : (UGame.Instance.uSex == 2 ? "Female" : "Unknown");
            }

            for (global::System.Int32 i = 0; i < UGame.Instance.other_users.Count; i++)
            {
                if (UGame.Instance.other_users[i].Teamid == WinnerTeamid)
                {
                    GameObject matchItem = Instantiate(PlayerCardPrefab);
                    matchItem.transform.SetParent(PlayerCardHolder.content, false);
                    PlayerCardHolder.content.sizeDelta = new Vector2(PlayerCardHolder.content.sizeDelta.x, PlayerCardHolder.content.sizeDelta.y + 106);
                    matchItem.transform.Find("name").GetComponent<Text>().text = UGame.Instance.other_users[i].Unick;
                    matchItem.transform.Find("header/avator").GetComponent<Image>().sprite = sysavatars[UGame.Instance.other_users[i].Usysavatar % sysavatars.Length];
                    matchItem.transform.Find("sex").GetComponent<Text>().text = UGame.Instance.other_users[i].Usex == 1 ? "Male" : (UGame.Instance.other_users[i].Usex == 2 ? "Female" : "Unknown");
                }
            }
        }
    }

    public void OnQuitGameButtonClicked()
    {
        LogicServiceProxy.Instance.QuitMatch();
    }
}
