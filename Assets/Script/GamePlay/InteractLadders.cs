using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractLadders : MonoBehaviour
{
    private float vertical;
    public float moveSpeed = 5f;
    private bool onLadder = false;
    private bool climbing = false;

    [SerializeField] private new Rigidbody2D rigidbody;

    // Update is called once per frame
    void Update()
    {
        vertical = Input.GetAxis("Vertical");

        if (onLadder && Mathf.Abs(vertical) > 0f)
        {
            climbing = true;
        }
    }

    private void FixedUpdate()
    {
        if (climbing)
        {
            if (vertical != 0f)
            {
                rigidbody.gravityScale = 0f;
                rigidbody.velocity = new Vector2(rigidbody.velocity.x, vertical * moveSpeed);
            }
            else
            {
                rigidbody.gravityScale = 0.05f;
            }
        }
        else
        {
            rigidbody.gravityScale = 2f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            onLadder = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            onLadder = false;
            climbing = false;
        }
    }
}