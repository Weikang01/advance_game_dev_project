using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractLadders : MonoBehaviour
{
    private float vertical;
    public float moveSpeed = 5f;
    private bool onLadder = false;
    public bool climbing = false;

    public Collider2D ground;
    private Collider2D ladder;

    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private LayerMask jumpableGround;

    public Collider2D feet;
    public Collider2D head;
    public Collider2D body;
    public Collider2D main;
    public bool onGround = false;
    public bool check = false;

    private void Start()
    {
        ground = GameObject.FindGameObjectWithTag("Ground").GetComponent<Collider2D>();
        ladder = GameObject.FindGameObjectWithTag("Ladder").GetComponent<Collider2D>();

        head = gameObject.transform.Find("Head").gameObject.GetComponent<Collider2D>();
        feet = gameObject.transform.Find("Feet").gameObject.GetComponent<Collider2D>();
        body = gameObject.GetComponent<BoxCollider2D>();
        main = gameObject.GetComponent<CapsuleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        vertical = Input.GetAxis("Vertical");

        if (onLadder && Mathf.Abs(vertical) > 0f)
        {
            climbing = true;
        }

        if ((feet.IsTouching(ladder) && onGround && climbing) ||
            (head.IsTouching(ladder) && head.IsTouching(ground) && climbing) ||
            (body.IsTouching(ladder) && body.IsTouching(ground) && climbing))
        {
            check = true;

            Physics2D.IgnoreCollision(main, ground, true);
        }
        else if ((!climbing && !body.IsTouching(ground)) || !onGround)
        {
            check = false;

            Physics2D.IgnoreCollision(main, ground, false);
        }
    }

    private void FixedUpdate()
    {
        if (climbing)
        {
            if (vertical != 0f)
            {
                rigidbody.gravityScale = 0f;
                rigidbody.velocity = new Vector2(rigidbody.velocity.x * 0.5f, 
                                                 vertical * moveSpeed);
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") && !head.IsTouching(ground))
        {
            onGround = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            onGround = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            onLadder = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
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
