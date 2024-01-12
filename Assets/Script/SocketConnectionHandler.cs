using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



public class SocketConnectionHandler : MonoBehaviour
{
    private short clientID;
    private GameSocket gameSocket;
    private float mSynchronous = 0;
    
    public GameObject player;
    private Dictionary<short, GameObject> otherPlayers;


    // Start is called before the first frame update
    void Start()
    {
        clientID = (short)UnityEngine.Random.Range(0, Int16.MaxValue);
        gameSocket = GameSocket.GetInstance();
        otherPlayers = new Dictionary<short, GameObject>();

        SendPlayerWorldMessage();
    }

    // Update is called once per frame
    void Update()
    {
        SendPlayerWorldMessage();
        ReceiveOtherPlayerWorldMessage();
    }

    private void ReceiveOtherPlayerWorldMessage()
    {
        mSynchronous += Time.deltaTime;

        if (mSynchronous > 0.5f)
        {
            foreach (KeyValuePair<short, List<GameMessage.IngameMessage>> entry in gameSocket.messages)
            {
                foreach (GameMessage.IngameMessage worldMessage in entry.Value)
                {
                    if (!otherPlayers.ContainsKey(entry.Key))
                    {
                        otherPlayers.Add(entry.Key, Instantiate(player, new Vector3(worldMessage.playerPosX + 1, worldMessage.playerPosY + 1, 0), Quaternion.identity));
                    }
                    else
                    {
                        otherPlayers[entry.Key].transform.position = new Vector3(worldMessage.playerPosX + 1, worldMessage.playerPosY + 1, 0);
                    }
                }
                entry.Value.Clear();
            }

            mSynchronous = 0;
        }
    }

    private void SendPlayerWorldMessage()
    {
        gameSocket.SendMessage(clientID, new GameMessage.IngameMessage(player.transform.position.x, player.transform.position.y));
    }
}
