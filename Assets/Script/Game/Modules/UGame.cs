using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInfo
{
    public string uNick;
    public int uSex;
    public int uSystemAvatar;
}

public class UGame : Singleton<UGame>
{
    public string uNick = null;
    public int uSystemAvatar = 0;
    public int uSex = 0;
    public int uVip = 0;
    public bool is_guest = false;
    public string guest_key = null;

    public GetUGameInfo uGameInfo = null;
    public int zoneid = -1;
    public int matchid = -1;
    public int self_teamid = -1;
    public int self_seatid = -1;
    public List<OnOtherEnteredMatch> other_users = new List<OnOtherEnteredMatch>();
    public List<CharacterInfo> match_players_info = new List<CharacterInfo>();

    public void SaveUInfo(UserCenterInfo uinfo, bool is_guest, string guest_key)
    {
        this.uNick = uinfo.Unick;
        this.uSystemAvatar = uinfo.Usysavatar;
        this.uSex = uinfo.Usex;
        this.uVip = uinfo.Uvip;
        this.is_guest = is_guest;
        this.guest_key = guest_key;
    }

    public void SaveEditProfile(string unick, int usysavatar, int usex)
    {
        this.uNick = unick;
        this.uSystemAvatar = usysavatar;
        this.uSex = usex;
    }

    public void QuitMatch()
    {
        this.zoneid = -1;
        this.matchid = -1;
        this.self_teamid = -1;
        this.self_seatid = -1;
        this.other_users.Clear();
        this.match_players_info.Clear();
    }

    public void UserLogout()
    {
        uNick = null;
        uSystemAvatar = 0;
        uSex = 0;
        uVip = 0;
        is_guest = false;
        guest_key = null;
    }

    public void SaveUGameInfo(GetUGameInfo ugameinfo)
    {
        this.uGameInfo = ugameinfo;
    }

    public UserInfo GetUserInfo(int seatid)
    {
        UserInfo uinfo = new UserInfo();
        if (seatid == self_seatid)
        {
            uinfo.uNick = this.uNick;
            uinfo.uSex = this.uSex;
            uinfo.uSystemAvatar = this.uSystemAvatar;
            return uinfo;
        }
        else
        {
            for (int i = 0; i < this.other_users.Count; i++)
            {
                if (this.other_users[i].Seatid == seatid)
                {
                    uinfo.uNick = this.other_users[i].Unick;
                    uinfo.uSex = this.other_users[i].Usex;
                    uinfo.uSystemAvatar = this.other_users[i].Usysavatar;
                    return uinfo;
                }
            }
        }
        return null;
    }

    public CharacterInfo GetInMatchCharacterInfo(int seatid)
    {
        for (int i = 0; i < this.match_players_info.Count; i++)
        {
            if (this.match_players_info[i].Seatid == seatid)
            {
                return this.match_players_info[i];
            }
        }
        return null;
    }
}
