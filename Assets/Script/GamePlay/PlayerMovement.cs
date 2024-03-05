using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public bool isCurrentPlayer = false;
    private Rigidbody2D rb;
    private CapsuleCollider2D coll;
    private SpriteRenderer sprite;
    public SocketConnectionHandler socketConnectionHandler;

    [SerializeField] private LayerMask jumpableGround;

    internal float dirX = 0f;
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 14f;
    internal float tempSpeed = 0f;
    internal float tempJump = 0f;

    //Vector3 move;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<CapsuleCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        tempSpeed = moveSpeed;
        tempJump = jumpForce;

        GameMessage.clientMessage ingameMessage = new GameMessage.clientMessage();
        ingameMessage.playerPosX = transform.position.x;
        ingameMessage.playerPosY = transform.position.y;
        ingameMessage.actionType = (short)GameMessage.ActionType.ENTER;
        if (isCurrentPlayer)
            socketConnectionHandler.SendPlayerWorldMessage(ingameMessage);
    }

    // Update is called once per frame
    void Update()
    {
        if (isCurrentPlayer)
        {
            dirX = Input.GetAxisRaw("Horizontal");

            //move = new Vector3(Input.GetAxis("Horizontal"), 0f, 0f);

            if (dirX != 0)
            {
                Move(dirX);

                GameMessage.clientMessage ingameMessage = new GameMessage.clientMessage();

                ingameMessage.playerPosX = transform.position.x;
                ingameMessage.playerPosY = transform.position.y;
                ingameMessage.faceDirection = dirX;
                ingameMessage.actionType = (short)GameMessage.ActionType.MOVE;
                socketConnectionHandler.SendPlayerWorldMessage(ingameMessage);
            }

            if (Input.GetButtonDown("Jump") && IsGrounded())
            {
                Jump();

                GameMessage.clientMessage ingameMessage = new GameMessage.clientMessage();
                ingameMessage.playerPosX = transform.position.x;
                ingameMessage.playerPosY = transform.position.y;
                ingameMessage.actionType = (short)GameMessage.ActionType.JUMP;
                socketConnectionHandler.SendPlayerWorldMessage(ingameMessage);
            }

            if (Input.GetButtonDown("Crouch") && IsGrounded())
            {
                Crouching();
            }

            if(Input.GetButtonUp("Crouch") && moveSpeed == 0f)
            {
                Standing();
            }
        }
    }

    public void Move(float face_direction)
    {
        dirX = face_direction;
        //rb.AddForce(new Vector2(face_direction * moveSpeed, rb.velocity.y) * rb.mass);
        rb.velocity = new Vector2(face_direction * moveSpeed, rb.velocity.y);

        //transform.position += moveSpeed * Time.fixedDeltaTime * move;
    }

    public void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        //rb.AddForce(new Vector2(rb.velocity.x, jumpForce) * rb.mass);
    }

    private void FixedUpdate()
    {
        if (dirX < 0)
        {
            sprite.flipX = true;
        }
        else if (dirX > 0)
        {
            sprite.flipX = false;
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.CapsuleCast(coll.bounds.center, coll.bounds.size, coll.direction, 0.0f, Vector2.down, .1f, jumpableGround);
    }

    private void Crouching()
    {
        moveSpeed = 0f;
        jumpForce = 0f;
        rb.mass = 10000f;
    }

    private void Standing()
    {
        moveSpeed = tempSpeed;
        jumpForce = tempJump;
        rb.mass = 10f;
    }
}
