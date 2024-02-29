using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInfoDlg : MonoBehaviour
{
    public TMP_InputField unickInput = null;
    public GameObject accountUpgradeBtn = null;
    public Image avatar = null;
    public Sprite[] sysAvatars = null;
    public Toggle maleToggle = null;
    public Toggle femaleToggle = null;
    // make group in inspector
    [Header("AvatarSelectPanel")]
    public GameObject avatarSelectPanel = null;
    [Header("AccountUpgradePanel")]
    public GameObject accountUpgradePanel = null;
    public TMP_InputField unameInput = null;
    public TMP_InputField upwdInput = null;
    public TMP_InputField upwd2Input = null;

    private int avatar_id = 0;

    public void OnSyncUInfo(string name, object udata)
    {
        if (UGame.Instance.is_guest)
        {
            this.accountUpgradeBtn.SetActive(true);
        }
        else
        {
            this.accountUpgradeBtn.SetActive(false);
        }

        if (UGame.Instance.uNick != null)
        {
            this.unickInput.text = UGame.Instance.uNick;
        }
        if (UGame.Instance.uSystemAvatar >= 0)
        {
            this.avatar_id = UGame.Instance.uSystemAvatar % this.sysAvatars.Length;
            this.avatar.sprite = this.sysAvatars[avatar_id];
        }

        if (maleToggle && femaleToggle)
        {
            switch (UGame.Instance.uSex)
            {
                case 0:
                    maleToggle.isOn = false;
                    femaleToggle.isOn = false;
                    break;
                case 1:
                    maleToggle.isOn = true;
                    break;
                case 2:
                    femaleToggle.isOn = true;
                    break;
            }
        }
    }

    public void OnUpdateAccountRet(string name, object udata)
    {
        int status = (int)udata;

        if (status == Responses.OK)
        {
            OnAccountUpgradeClose();
            this.accountUpgradeBtn.SetActive(false);
        }
        else
        {
            Debug.Log("Update Account failed! Error: " + status);
        }
    }

    private void OnEnable()
    {
        OnSyncUInfo("SyncUInfo", null);
        EventManager.Instance.AddEventListener("SyncUInfo", OnSyncUInfo);
        EventManager.Instance.AddEventListener("OnUpdateAccountRet", OnUpdateAccountRet);
    }

    public void OnAvatarSelectBtnClicked()
    {
        if (avatarSelectPanel)
        {
            avatarSelectPanel.SetActive(true);
        }
    }

    public void OnUInfoPanelClose()
    {
        //this.gameObject.SetActive(false);
        GameObject.Destroy(this.gameObject);
    }

    public void OnAvatarSelectClose()
    {
        this.avatarSelectPanel.SetActive(false);
    }

    public void OnAvatarSelectAvatarImageClicked(int avatar_id)
    {
        this.avatar_id = avatar_id;
        if (avatar_id < this.sysAvatars.Length)
        {
            this.avatar.sprite = this.sysAvatars[avatar_id];
        }
        this.avatarSelectPanel.SetActive(false);
    }

    public void OnEditProfileCommit()
    {
        if (this.unickInput.text.Length <= 0)
            return;

        // TODO: send message to server
        int sex = this.maleToggle.isOn ? 1 : this.femaleToggle.isOn? 2 : 0;

        //Debug.Log("usex: " + sex + "\tunick: " + this.unickInput.text + "\tavatar id: " + this.avatar_id);
        AuthServiceProxy.Instance.EditProfile(this.unickInput.text, this.avatar_id, sex);
    }

    public void OnAccountUpgradeBtnClicked()
    {
        AuthServiceProxy.Instance.UpgradeAccount(this.unameInput.text, Utils.Md5(this.upwdInput.text));
    }

    public void OnAccountUpgradeClose()
    {
        if (accountUpgradePanel)
        {
            accountUpgradePanel.SetActive(false);
        }
    }

    private bool is_valid_uname = false;
    private bool is_valid_pwd = false;

    public void OnUnameValueChanged(Button accountUpgradeBtn)
    {
        if (accountUpgradeBtn)
        {
            if (unameInput.text.Length <= 0)
            {
                is_valid_uname = false;
            }
            else
            {
                is_valid_uname = true;
            }

            accountUpgradeBtn.interactable = is_valid_uname && is_valid_pwd;
        }
    }

    public void OnPasswordValueChanged(Button accountUpgradeBtn)
    {
        if (accountUpgradeBtn)
        {
            if (upwdInput.text != upwd2Input.text)
            {
                is_valid_pwd = false;
            }
            else
            {
                is_valid_pwd = true;
            }

            accountUpgradeBtn.interactable = is_valid_uname && is_valid_pwd;
        }
    }

    public void OnAccountUpgradeOpen()
    {
        if (accountUpgradePanel)
        {
            accountUpgradePanel.SetActive(true);
        }
    }

    public void OnLogoutButtonClick()
    {
        AuthServiceProxy.Instance.UserLogout();
    }

    private void OnDestroy()
    {
        EventManager.Instance.RemoveEventListener("SyncUInfo", OnSyncUInfo);
        EventManager.Instance.RemoveEventListener("OnUpdateAccountRet", OnUpdateAccountRet);
    }
}
