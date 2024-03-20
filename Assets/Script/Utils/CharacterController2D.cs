using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CharacterController2D : MonoBehaviour
{
    private float m_Speed = 5.0f;
    private float m_JumpForce = 10.0f;                          // Amount of force added when the player jumps.
    private float m_ClimbSpeed = 1.0f;                          // Amount of force added when the player climbs.
    [Range(0, 1)][SerializeField] private float m_CrouchSpeed = .36f;           // Amount of maxSpeed applied to crouching movement. 1 = 100%
    [Range(0, .3f)][SerializeField] private float m_MovementSmoothing = .05f;   // How much to smooth out the movement
    private bool m_AirControl = true;                                           // Whether or not a player can steer while jumping;
    [SerializeField] private LayerMask m_WhatIsGround;                          // A mask determining what is ground to the character
    [SerializeField] private Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
    [SerializeField] private Transform m_CeilingCheck;                          // A position marking where to check for ceilings
    [SerializeField] private Collider2D m_CrouchDisableCollider;                // A collider that will be disabled when crouching

    const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
    public bool Grounded;            // Whether or not the player is grounded.
    const float k_CeilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up
    private Rigidbody2D m_Rigidbody2D;
    public bool FacingRight = true;  // For determining which way the player is currently facing.
    private Vector3 m_Velocity = Vector3.zero;
    public bool OnLadder = false;

    [Header("Events")]
    [Space]

    public UnityEvent OnLandEvent;

    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public BoolEvent OnCrouchEvent;
    private bool m_wasCrouching = false;

    private void Awake()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        if (OnCrouchEvent == null)
            OnCrouchEvent = new BoolEvent();
    }

    private void FixedUpdate()
    {
        bool wasGrounded = Grounded;
        //m_Grounded = false;
        Grounded = true;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                Grounded = true;
                if (!wasGrounded)
                    OnLandEvent.Invoke();
            }
        }
    }

    public void Move(float move, float climb, bool jump, bool crouch, float dt)
    {
        //only control the player if grounded or airControl is turned on
        if (Grounded || m_AirControl || OnLadder)
        {
            //this.gameObject.transform.position += new Vector3(move, climb, 0);
            m_Rigidbody2D.velocity = new Vector2(
                move,
                climb != 0.0f ? climb : m_Rigidbody2D.velocity.y);


            if (move > 0 && !FacingRight)
            {
                // ... flip the player.
                Flip();
            }
            else if (move < 0 && FacingRight)
            {
                Flip();
            }
        }
        // If the player should jump...
        //Debug.Log("Grounded: " + Grounded + " Jump: " + jump);
        if (jump)
        {
            // Add a vertical force to the player.
            Grounded = false;
            //m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
            //this.gameObject.transform.position += new Vector3(0, m_JumpForce * dt, 0);
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, m_JumpForce);
        }
    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        FacingRight = !FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            this.m_Rigidbody2D.gravityScale = 0;
            this.m_Rigidbody2D.velocity = Vector3.zero;
            OnLadder = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ladder"))
        {
            this.m_Rigidbody2D.gravityScale = 1;
            this.m_Rigidbody2D.velocity = Vector3.zero;
            OnLadder = false;
        }
    }
}
