using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
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

        EventManager.Instance.DispatchEvent("enterZoneSuccess", null);
    }

    void OnEnterMatch(cmd_msg msg)
    {
        EnterMatch res = ProtoManager.DeserializeProtobuf<EnterMatch>(msg.body);
        if (msg.body == null)
            return;

        UGame.Instance.zoneid = res.Zid;
        UGame.Instance.matchid = res.Matchid;
        UGame.Instance.self_teamid = res.Teamid;
        UGame.Instance.self_seatid = res.Seatid;
        UGame.Instance.other_users = res.OtherUinfo.ToList();
        EventManager.Instance.DispatchEvent("enterMatchSuccess", res);
    }

    void OtherEnteredMatch(cmd_msg msg)
    {
        OnOtherEnteredMatch res = ProtoManager.DeserializeProtobuf<OnOtherEnteredMatch>(msg.body);
        if (msg.body == null)
            return;

        UGame.Instance.other_users.Add(res);
        EventManager.Instance.DispatchEvent("otherEnteredMatch", res);
    }

    void OnQuitMatch(cmd_msg msg)
    {
        QuitMatchRes res = ProtoManager.DeserializeProtobuf<QuitMatchRes>(msg.body);
        if (msg.body == null)
            return;
        if (res.Status != Responses.OK)
        {
            Debug.Log("Quit match failed with status code " + res.Status);
            return;
        }
        EventManager.Instance.DispatchEvent("quitMatchSuccess", null);
        UGame.Instance.zoneid = -1;
        UGame.Instance.other_users.Clear();
    }

    void OnOtherQuittedMatch(cmd_msg msg)
    {
        OnOtherQuittedMatch res = ProtoManager.DeserializeProtobuf<OnOtherQuittedMatch>(msg.body);
        if (msg.body == null)
            return;

        for (int i = 0; i < UGame.Instance.other_users.Count; i++)
        {
            if (UGame.Instance.other_users[i].Seatid == res.Seatid)
            {
                UGame.Instance.other_users.RemoveAt(i);
                EventManager.Instance.DispatchEvent("otherQuittedMatch", i);
                break; // Exit the loop once the item is found
            }
        }
    }

    void OnGameStart(cmd_msg msg)
    {
        GameStart res = ProtoManager.DeserializeProtobuf<GameStart>(msg.body);
        if (msg.body == null)
            return;

        UGame.Instance.match_players_info = res.Characters.ToList();

        EventManager.Instance.DispatchEvent("gameStart", res);
    }

    void OnLogicFrame(cmd_msg msg)
    {
        LogicFrame res = ProtoManager.DeserializeProtobuf<LogicFrame>(msg.body);
        if (msg.body == null)
            return;

        EventManager.Instance.DispatchEvent("onLogicUpdate", res);
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
            case (int)Cmd.EEnterMatch:
                OnEnterMatch(msg);
                break;
            case (int)Cmd.EOnOtherEnteredMatch:
                OtherEnteredMatch(msg);
                break;
            case (int)Cmd.EQuitMatchRes:
                OnQuitMatch(msg);
                break;
            case (int)Cmd.EOnOtherQuittedMatch:
                OnOtherQuittedMatch(msg);
                break;
            case (int)Cmd.EGameStart:
                OnGameStart(msg);
                break;
            case (int)Cmd.ELogicFrame:
                OnLogicFrame(msg);
                break;
        }
    }

    public void Init()
    {
        Network.Instance.AddServiceListener((int)Stype.ELogic, OnLogicServerReturn);
    }

    public void LogicLogin()
    {
        LogicLoginReq req = new LogicLoginReq();
        req.UdpIp = "127.0.0.1";
        req.UdpPort = Network.Instance.local_udp_port;
        Network.Instance.SendProtoBufCmd((int)Stype.ELogic, (int)Cmd.ELogicLoginReq, req);
    }

    public void EnterZone(int zid)
    {
        if (zid != Zones.zone1 && zid != Zones.zone2)
        {
            return;
        }

        EnterZoneReq req = new EnterZoneReq();
        req.Zid = zid;
        Network.Instance.SendProtoBufCmd((int)Stype.ELogic, (int)Cmd.EEnterZoneReq, req);
    }

    public void QuitMatch()
    {
        Network.Instance.SendProtoBufCmd((int)Stype.ELogic, (int)Cmd.EQuitMatchReq, null);
    }

    public void SendNextFrameOpts(NextFrameOpt nextFrameOpt)
    {
        Network.Instance.UDPSendProtoBufCmd((int)Stype.ELogic, (int)Cmd.ENextFrameOpt, nextFrameOpt);
    }
}
