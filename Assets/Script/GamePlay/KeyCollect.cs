using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCollect : MonoBehaviour
{
    GameObject Key;
    GameObject Player;
    GameObject Gate;
    public bool collected = false;

    // Start is called before the first frame update
    void Start()
    {
        Key = GameObject.Find("Key");
        Player = GameObject.Find("Player");
        Gate = GameObject.Find("GoldGate");
    }

    void FixedUpdate()
    {
        if (collected)
        {
            float x = Player.transform.position.x + 1;
            float y = Player.transform.position.y;
            Key.transform.position = new Vector3(x, y, 0);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collected = true;
        }

        if (collision.gameObject.CompareTag("GoldGate"))
        {
            Key.SetActive(false);
            Gate.SetActive(false);
        }
    }
}
