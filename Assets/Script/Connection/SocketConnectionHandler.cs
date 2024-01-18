using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SocketConnectionHandler : MonoBehaviour
{
    private GameSocket gameSocket;
    private ClientData clientData;
    public GameObject player;
    private Dictionary<short, GameObject> otherPlayers;

    // Start is called before the first frame update
    void Start()
    {
        clientData = ClientData.GetInstance();
        gameSocket = GameSocket.GetInstance();
        otherPlayers = new Dictionary<short, GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        ReceiveOtherPlayerWorldMessage();
    }

    private void ReceiveOtherPlayerWorldMessage()
    {
        foreach (KeyValuePair<short, List<GameMessage.clientMessage>> entry in gameSocket.client_messages)
        {
            foreach (GameMessage.clientMessage ingameMessage in entry.Value)
            {
                Debug.Log("Action type: " + ingameMessage.actionType);

                if (!otherPlayers.ContainsKey(entry.Key))
                {
                    otherPlayers.Add(entry.Key, Instantiate(player, new Vector3(ingameMessage.playerPosX + 2, ingameMessage.playerPosY, 0), Quaternion.identity));
                    otherPlayers[entry.Key].GetComponent<PlayerMovement>().isCurrentPlayer = false;
                }
                else
                {
                    switch (ingameMessage.actionType)
                    {
                        case (short)GameMessage.ActionType.MOVE:
                            otherPlayers[entry.Key].GetComponent<PlayerMovement>().Move(ingameMessage.faceDirection);
                            break;
                        case (short)GameMessage.ActionType.JUMP:
                            otherPlayers[entry.Key].GetComponent<PlayerMovement>().Jump();
                            break;
                    }
                }
            }
            entry.Value.Clear();
        }
    }

    public void SendPlayerWorldMessage(GameMessage.clientMessage ingameMessage)
    {
        gameSocket.SendMessage(clientData.clientID, ingameMessage);
    }
}
