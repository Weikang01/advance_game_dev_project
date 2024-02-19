using System;
using UnityEngine;

public class AuthServiceProxy : Singleton<AuthServiceProxy>
{
    private string g_key = null;
    private bool is_save_gkey = false;

    private EditProfileReq tempEditProfileReq = null;

    // create a lock for edit profile
    private bool profile_lock = false;


    private void OnGuestLoginReturn(cmd_msg msg)
    {
        GuestLoginRes res = ProtoManager.DeserializeProtobuf<GuestLoginRes>(msg.body);
        if (res == null)
            return;

        if (res.Status != Responses.OK)
        {
            Debug.Log("Guest login failed with status code " + res.Status);
            return;
        }

        UGame.Instance.SaveUInfo(res.Uinfo, true, this.g_key);

        if (this.is_save_gkey)
        {
            this.is_save_gkey = false;
            PlayerPrefs.SetString("guest_key", this.g_key);
        }

        EventManager.Instance.DispatchEvent("loginSuccess", null);
        EventManager.Instance.DispatchEvent("SyncUInfo", null);
    }

    private void OnReloginReturn(cmd_msg msg)
    {
        Debug.Log("Another device login with the same account");
    }

    private void OnEditProfileReturn(cmd_msg msg)
    {
        EditProfileRes res = ProtoManager.DeserializeProtobuf<EditProfileRes>(msg.body);
        if (res == null)
            return;
        if (res.Status != Responses.OK)
        {
            Debug.Log("Edit profile failed with status code " + res.Status);
            return;
        }

        UGame.Instance.SaveEditProfile(tempEditProfileReq.Unick, tempEditProfileReq.Usysavatar, tempEditProfileReq.Usex);
        EventManager.Instance.DispatchEvent("SyncUInfo", null);

        this.profile_lock = false;
    }

    private void OnGuestUpgradeReturn(cmd_msg msg)
    {
        GuestUpgradeRes res = ProtoManager.DeserializeProtobuf<GuestUpgradeRes>(msg.body);

        if (res == null)
            return;

        if (res.Status != Responses.OK)
        {
            Debug.Log("Guest upgrade failed with status code " + res.Status);
            return;
        }

        UGame.Instance.is_guest = false;
        EventManager.Instance.DispatchEvent("OnUpdateAccountRet", res.Status);
        PlayerPrefs.DeleteKey("guest_key");
    }

    private void OnUserLoginReturn(cmd_msg msg)
    {
        UserLoginRes res = ProtoManager.DeserializeProtobuf<UserLoginRes>(msg.body);
        if (res == null)
            return;
        if (res.Status != Responses.OK)
        {
            Debug.Log("User login failed with status code " + res.Status);
            return;
        }
        UGame.Instance.SaveUInfo(res.Uinfo, false, null);
        EventManager.Instance.DispatchEvent("loginSuccess", null);
        EventManager.Instance.DispatchEvent("SyncUInfo", null);
    }

    private void OnLogoutReturn(cmd_msg msg)
    {
        LogoutRes res = ProtoManager.DeserializeProtobuf<LogoutRes>(msg.body);
        if (res == null)
            return;
        if (res.Status != Responses.OK)
        {
            Debug.Log("Logout failed with status code " + res.Status);
            return;
        }

        UGame.Instance.UserLogout();
        EventManager.Instance.DispatchEvent("logout", null);
    }

    void OnUserLogin(cmd_msg msg)
    {
        switch (msg.ctype)
        {
            case (int)Cmd.EGuestLoginRes:
                OnGuestLoginReturn(msg);
                break;
            case (int)Cmd.EReloginRes:
                OnReloginReturn(msg);
                break;
            case (int)Cmd.EEditProfileRes:
                OnEditProfileReturn(msg);
                break;
            case (int)Cmd.EGuestUpgradeRes:
                OnGuestUpgradeReturn(msg);
                break;
            case (int)Cmd.EUserLoginRes:
                OnUserLoginReturn(msg);
                break;
            case (int)Cmd.ELogoutRes:
                OnLogoutReturn(msg);
                break;
        }
    }

    public void Init()
    {
        Network.Instance.AddServiceListener((int)Stype.EAuth, OnUserLogin);
    }

    public void GuestLogin()
    {
        if (!PlayerPrefs.HasKey("guest_key"))
        {
            this.g_key = Utils.RandomString(32);
            PlayerPrefs.SetString("guest_key", this.g_key);
            this.is_save_gkey = true;
        }
        else
        {
            this.g_key = PlayerPrefs.GetString("guest_key");
        }

        GuestLoginReq req = new GuestLoginReq();
        req.GuestKey = this.g_key;

        Network.Instance.sendProtoBufCmd((int)Stype.EAuth, (int)Cmd.EGuestLoginReq, req);
    }

    public void EditProfile(string unick, int uavatar, int usex)
    {
        if (unick.Length <= 0 || uavatar < 0 || uavatar >= 9 || usex < 0 || usex > 2 || this.profile_lock)
            return;

        this.profile_lock = true;

        this.tempEditProfileReq = new EditProfileReq();
        this.tempEditProfileReq.Unick = unick;
        this.tempEditProfileReq.Usysavatar = uavatar;
        this.tempEditProfileReq.Usex = usex;

        Network.Instance.sendProtoBufCmd((int)Stype.EAuth, (int)Cmd.EEditProfileReq, this.tempEditProfileReq);
    }

    public void UpgradeAccount(string uname, string md5_pwd)
    {
        GuestUpgradeReq req = new GuestUpgradeReq();
        req.Uname = uname;
        req.UpwdMd5 = md5_pwd;

        Network.Instance.sendProtoBufCmd((int)Stype.EAuth, (int)Cmd.EGuestUpgradeReq, req);
    }

    internal void UserAccountLogin(string uname, string upwd)
    {
        string md5_pwd = Utils.Md5(upwd);

        //Debug.Log(uname + " " + md5_pwd);
        UserLoginReq req = new UserLoginReq();
        req.Uname = uname;
        req.UpwdMd5 = md5_pwd;

        Network.Instance.sendProtoBufCmd((int)Stype.EAuth, (int)Cmd.EUserLoginReq, req);
    }

    internal void UserLogout()
    {
        Network.Instance.sendProtoBufCmd((int)Stype.EAuth, (int)Cmd.ELogoutReq, null);
    }
}
