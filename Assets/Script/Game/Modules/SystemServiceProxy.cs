using System;
using UnityEngine;

public class SystemServiceProxy : Singleton<SystemServiceProxy>
{
    private void OnGetUgameInfoReturn(cmd_msg msg)
    {
        GetUGameInfoRes res = ProtoManager.DeserializeProtobuf<GetUGameInfoRes>(msg.body);

        if (msg.body == null)
            return;

        if (res.Status != Responses.OK)
        {
            Debug.Log("Get ugame info failed with status code " + res.Status);
            return;
        }

        GetUGameInfo uinfo = res.Uinfo;

        //Debug.Log("Get ugame info success: Uchip: " + uinfo.Uchip + " Uexp: " + uinfo.Uexp + " Uvip: " + uinfo.Uvip);
        UGame.Instance.SaveUGameInfo(uinfo);

        EventManager.Instance.DispatchEvent("getUgameInfo", null);
        EventManager.Instance.DispatchEvent("SyncUGameInfo", null);
    }

    void OnSystemServerReturn(cmd_msg msg)
    {
        switch (msg.ctype)
        {
            case (int)Cmd.EGetUgameInfoRes:
                OnGetUgameInfoReturn(msg);
                break;
        }
    }

    public void Init()
    {
        Network.Instance.AddServiceListener((int)Stype.ESystem, OnSystemServerReturn);
    }

    public void GetUgameInfo()
    {
        Network.Instance.sendProtoBufCmd((int)Stype.ESystem, (int)Cmd.EGetUgameInfoReq, null);
    }
}
