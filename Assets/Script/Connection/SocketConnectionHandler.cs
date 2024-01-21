using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SocketConnectionHandler : MonoBehaviour
{
    public GameObject player;

    private GameSocket gameSocket;
    private gameData gameData;
    

    // Start is called before the first frame update
    void Start()
    {
        gameData = gameData.GetInstance();
        gameSocket = GameSocket.GetInstance();

        // receive other player world message
        StartCoroutine(ReceiveSystemMessage());
        StartCoroutine(ReceiveClientMessage());
    }

    private IEnumerator ReceiveClientMessage()
    {
        while (true) // You may need a condition to exit the loop
        {
            foreach (KeyValuePair<short, List<GameMessage.clientMessage>> entry in gameSocket.client_messages)
            {
                // Create a copy of the list to avoid modifying it during iteration
                List<GameMessage.clientMessage> copyOfMessages = new List<GameMessage.clientMessage>(entry.Value);

                foreach (GameMessage.clientMessage ingameMessage in copyOfMessages)
                {
                    Debug.Log("Action type: " + ingameMessage.actionType);

                    if (!gameData.players.ContainsKey(entry.Key))
                    {
                        gameData.AddOtherClientID(entry.Key, Instantiate(player, new Vector3(ingameMessage.playerPosX + 2, ingameMessage.playerPosY, 0), Quaternion.identity));
                        gameData.players[entry.Key].GetComponent<PlayerMovement>().isCurrentPlayer = false;
                    }
                    else
                    {
                        switch (ingameMessage.actionType)
                        {
                            case (short)GameMessage.ActionType.MOVE:
                                gameData.players[entry.Key].GetComponent<PlayerMovement>().Move(ingameMessage.faceDirection);
                                break;
                            case (short)GameMessage.ActionType.JUMP:
                                gameData.players[entry.Key].GetComponent<PlayerMovement>().Jump();
                                break;
                        }
                    }
                }
                entry.Value.Clear();
            }
            yield return null;
        }
    }


    private IEnumerator ReceiveSystemMessage()
    {
        while (true) // You may need a condition to exit the loop
        {
            for (int i = 0; i < gameSocket.system_messages.Count; i++)
            {
                Dictionary<string, string> systemMessage = gameSocket.system_messages[i];

                Debug.Log("System action: " + systemMessage["action"]);
                switch (systemMessage["action"])
                {
                    case "create_string":
                        GameObject rope = Instantiate(Resources.Load("Prefabs/Rope", typeof(GameObject))) as GameObject;
                        rope.GetComponent<Rope>().endA = gameData.players[short.Parse(systemMessage["from"])];
                        rope.GetComponent<Rope>().endB = gameData.players[short.Parse(systemMessage["to"])];
                        break;
                }

                // Remove the processed dictionary from the list
                gameSocket.system_messages.RemoveAt(i);
                i--; // Decrement i because the list size has been reduced
            }

            // empty the list
            gameSocket.system_messages.Clear();

            yield return null;
        }
    }



    public void SendPlayerWorldMessage(GameMessage.clientMessage ingameMessage)
    {
        gameSocket.SendMessage(gameData.clientID, ingameMessage);
    }


    public void OnDestroy()
    {
        if (gameSocket != null)
        {
            gameSocket.Close(gameData.clientID);
        }
    }

    public void OnApplicationQuit()
    {
        if (gameSocket != null)
        {
            gameSocket.Close(gameData.clientID);
        }
    }
}
