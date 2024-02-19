using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
