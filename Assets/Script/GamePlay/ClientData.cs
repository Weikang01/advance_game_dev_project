using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientData
{
    internal short clientID;

    private ClientData()
    {
        clientID = (short)UnityEngine.Random.Range(0, Int16.MaxValue);
    }

    private static ClientData instance;

    public static ClientData GetInstance()
    {
        if (instance == null)
        {
            instance = new ClientData();
        }
        return instance;
    }
}
