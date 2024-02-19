using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;

public class LogicServiceProxy : Singleton<LogicServiceProxy>
{
    void OnLogicLogin(cmd_msg msg)
    {
        LogicLoginRes res = ProtoManager.DeserializeProtobuf<LogicLoginRes>(msg.body);
        if (msg.body == null)
            return;
        if (res.Status != Responses.OK)
        {
            Debug.Log("Logic login failed with status code " + res.Status);
            return;
        }
        EventManager.Instance.DispatchEvent("logicLoginSuccess", null);
    }

    void OnEnterZone(cmd_msg msg)
    {
        EnterZoneRes res = ProtoManager.DeserializeProtobuf<EnterZoneRes>(msg.body);
        if (msg.body == null)
            return;
        if (res.Status != Responses.OK)
        {
            Debug.Log("Enter zone failed with status code " + res.Status);
            return;
        }

        Debug.Log("Entered zone successfully!");
        EventManager.Instance.DispatchEvent("enterZoneSuccess", null);
    }

    void OnLogicServerReturn(cmd_msg msg)
    {
        switch (msg.ctype)
        {
            case (int)Cmd.ELogicLoginRes:
                OnLogicLogin(msg);
                break;
            case (int)Cmd.EEnterZoneRes:
                OnEnterZone(msg);
                break;
        }
    }

    public void Init()
    {
        Network.Instance.AddServiceListener((int)Stype.ELogic, OnLogicServerReturn);
    }

    public void LogicLogin()
    {
        Network.Instance.sendProtoBufCmd((int)Stype.ELogic, (int)Cmd.ELogicLoginReq, null);
    }

    public void EnterZone(int zid)
    {
        //if (zid != Zones.zone1 && zid != Zones.zone2)
        //{
        //    return;
        //}

        EnterZoneReq req = new EnterZoneReq();
        req.Zid = zid;
        Network.Instance.sendProtoBufCmd((int)Stype.ELogic, (int)Cmd.EEnterZoneReq, req);
    }
}
