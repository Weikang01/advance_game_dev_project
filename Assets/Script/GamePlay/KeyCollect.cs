using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyCollect : MonoBehaviour
{
    GameObject Key;
    GameObject Player;
    GameObject Gate;
    public bool collected = false;

    private float dirX = 0f;
    private bool left = false;
    private SpriteRenderer sprite;

    // Start is called before the first frame update
    void Start()
    {
        Key = GameObject.Find("Key");
        Player = GameObject.Find("Player");
        Gate = GameObject.Find("GoldGate");

        sprite = gameObject.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        dirX = Input.GetAxisRaw("Horizontal");
    }

    void FixedUpdate()
    {   
        if (collected)
        {
            if (dirX < 0)
            {
                left = true;
                sprite.flipX = true;
            }
            else if (dirX > 0)
            {
                left = false;
                sprite.flipX = false;
            }

            float x = Player.transform.position.x + 1;
            float y = Player.transform.position.y;

            if (left)
            {
                x = x - 2;
            }

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
