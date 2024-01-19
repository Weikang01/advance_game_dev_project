using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameData
{
    internal short clientID;
    internal Dictionary<short, GameObject> players;

    private gameData()
    {
        clientID = (short)UnityEngine.Random.Range(0, Int16.MaxValue);
        players = new Dictionary<short, GameObject>
        {
            { clientID, GameObject.Find("Player") }
        };
    }

    private static gameData instance;

    public static gameData GetInstance()
    {
        if (instance == null)
        {
            instance = new gameData();
        }
        return instance;
    }

    public void AddOtherClientID(short clientID, GameObject gameObject)
    {
        players.Add(clientID, gameObject);
    }

    public void RemoveOtherClientID(short clientID)
    {
        players.Remove(clientID);
    }

    public GameObject GetPlayer(short clientID)
    {
        return players[clientID];
    }
}
