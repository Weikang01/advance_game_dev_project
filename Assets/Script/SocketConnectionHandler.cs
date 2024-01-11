using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SocketConnectionHandler : MonoBehaviour
{
    private short clientID;
    private GameSocket gameSocket;
    private float mSynchronous = 0;
    
    public GameObject player;
    private Dictionary<short, GameObject> otherPlayers;
    private GameObject otherPlayer;


    // Start is called before the first frame update
    void Start()
    {
        clientID = (short)UnityEngine.Random.Range(0, Int16.MaxValue);
        gameSocket = GameSocket.GetInstance();
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
            int count = gameSocket.messages.Count;

            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    GameMessage.IngameMessage worldMessage = gameSocket.messages[i];
                    if (otherPlayer == null)
                    {
                        otherPlayer = Instantiate(player, new Vector3(worldMessage.playerPosX + 1, worldMessage.playerPosY + 1, 0), Quaternion.identity);
                    }
                    else
                    {
                        otherPlayer.transform.position = new Vector3(worldMessage.playerPosX + 1, worldMessage.playerPosY + 1, 0);
                    }
                }

                gameSocket.messages.Clear();
            }

            mSynchronous = 0;
        }
    }

    private void SendPlayerWorldMessage()
    {
        gameSocket.SendMessage(new GameMessage.IngameMessage(player.transform.position.x, player.transform.position.y));
    }
}
