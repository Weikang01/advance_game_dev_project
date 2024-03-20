using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

enum CharacterState
{
    Walk = 1,
    Free = 2,
    Idle = 3,
    Attack = 4,
    Attack2 = 5,
    Attack3 = 6,
    Skill = 7,
    Skill2 = 8,
    Skill3 = 9,
    Die = 10,
    Climb = 11,
    Jump = 12,
}

public class Player : MonoBehaviour
{
    public Vector3 spawn_point;
    public bool is_ghost;  // Is the character controlled by other player
    public float speed = 80.0f;  // Character speed

    internal CharacterController2D characterController;
    private Animator anim;
    private CharacterState anim_state = CharacterState.Idle;
    public Vector3 camera_offset;  // relative position of the camera
    public Vector2 pivot_offset;   // reset position of the character
    private float jump_timer = 0.0f;

    private float stick_x = 0;
    private float stick_y = 0;
    private bool is_jumping = false;
    private bool is_climbing = false;

    public bool is_dead = false;

    internal int rope_index = -1;

    private CharacterState logic_state = CharacterState.Idle;
    //private Vector3 logic_position;

    public int seat_id = -1;
    public int team_id = -1;

    // Start is called before the first frame update
    void Start()
    {
        this.characterController = GetComponent<CharacterController2D>();


        this.anim = GetComponent<Animator>();
        if (this.anim == null)
        {
            Debug.Assert(false);
        }

        if (!this.is_ghost)  // player's character
        {
            Camera.main.transform.position = this.transform.position + this.camera_offset;
        }

        this.anim.SetFloat("MoveSpeed", 0.0f);
    }

    public void LogicInit(Vector3 init_position)
    {
        this.stick_x = 0;
        //this.logic_position = init_position;
        this.logic_state = CharacterState.Idle;
    }


    void DoMovementEvent(float dt)
    {
        if (this.is_jumping)
        {
            this.logic_state = CharacterState.Jump;
        }
        else if (this.stick_y != 0)
        {
            this.logic_state = CharacterState.Climb;
        }
        else if (this.stick_x != 0)
        {
            this.logic_state = CharacterState.Walk;
        }
        else
        {
            this.logic_state = CharacterState.Idle;
        }

        this.characterController.Move(speed * this.stick_x * dt, 0.5f * speed * this.stick_y * dt, this.is_jumping, false, dt);
    }

    void DoJumpEvent(float dt)
    {
        this.logic_state = CharacterState.Jump;
        //this.characterController
    }

    void DoDieEvent()
    {
        this.stick_x = 0;
        this.stick_y = 0;
        this.is_jumping = false;
        this.is_climbing = false;

        // move player to their spawning point
        this.gameObject.transform.position = spawn_point;
        EventManager.Instance.DispatchEvent("playerRespawned", rope_index);

        this.is_dead = false;
    }

    void OnMovementAnimUpdate()
    {
        if (this.jump_timer >=0)
        {
            this.jump_timer -= Time.deltaTime;
        }


        if (this.is_jumping)
        {
            this.anim.SetBool("Jumping", true);
            this.anim_state = CharacterState.Jump;
            jump_timer = 0.7f;
        }
        else if (this.characterController.OnLadder && this.stick_y != 0)
        {
            this.anim.SetBool("Climbing", true);

            if (this.jump_timer <= 0)
            {
                this.anim.SetBool("Jumping", false);
                this.anim_state = CharacterState.Climb;
            }
        }

        else
        {
            this.anim.SetBool("Climbing", false);
            if (this.jump_timer <= 0)
            {
                this.anim.SetBool("Jumping", false);
            }

            if (this.stick_x == 0)
            {
                this.anim.SetFloat("MoveSpeed", 0.0f);
                if (this.anim_state == CharacterState.Walk)
                {
                    this.anim_state = CharacterState.Idle;
                }

                return;
            }

            if (this.anim_state == CharacterState.Idle)
            {
                this.anim.SetFloat("MoveSpeed", 1.0f);
                this.anim_state = CharacterState.Walk;
            }
        }

        this.DoMovementEvent(Time.deltaTime);

        if (!this.is_ghost)
            Camera.main.transform.position = this.transform.position + this.camera_offset;
    }


    // Update is called once per frame
    void Update()
    {
        OnMovementAnimUpdate();
    }

    public void HandleFrameEvent(OptionEvent optionEvent)
    {
        switch (optionEvent.OptType)
        {
            case (int)OptType.Move:
                HandleMovementEvent(optionEvent);
                break;
            case (int)OptType.Jump:
                HandleMovementEvent(optionEvent);
                break;
            case (int)OptType.Dead:
                HandleDeadEvent(optionEvent);
                break;
            default:
                break;
        }
    }

    internal void SyncLastLogicFrame(OptionEvent optionEvent)
    {
        switch (optionEvent.OptType)
        {
            case (int)OptType.Move:
                SyncLastMovementEvent(optionEvent);
                break;
            case (int)OptType.Jump:
                SyncLastMovementEvent(optionEvent);
                break;
            case (int)OptType.Dead:
                SyncLastDeadEvent(optionEvent);
                break;
            default:
                break;
        }
    }

    internal void JumpToNextFrame(OptionEvent optionEvent)
    {
        switch (optionEvent.OptType)
        {
            case (int)OptType.Move:
                JumpToNextMovementEvent(optionEvent);
                break;
            case (int)OptType.Jump:
                JumpToNextMovementEvent(optionEvent);
                break;
            case (int)OptType.Dead:
                JumpToNextDeadEvent(optionEvent);
                break;
            default:
                break;
        }
    }

    private void HandleMovementEvent(OptionEvent optionEvent)
    {
        if (optionEvent.OptType == (int)OptType.Jump)
        {
            this.logic_state = CharacterState.Jump;
        }
        else if (optionEvent.Y != 0)
        {
            this.logic_state = CharacterState.Climb;
        }
        else
        {
            if (BitConverter.ToSingle(BitConverter.GetBytes(optionEvent.X)) == 0.0f)
            {
                this.logic_state = CharacterState.Idle;
            }
            else
            {
                this.logic_state = CharacterState.Walk;
            }
        }
    }

    private void HandleDeadEvent(OptionEvent optionEvent)
    {
        this.logic_state = CharacterState.Idle;
    }

    private void SyncLastMovementEvent(OptionEvent optionEvent)
    {
        this.stick_x = BitConverter.ToSingle(BitConverter.GetBytes(optionEvent.X));
        this.stick_y = BitConverter.ToSingle(BitConverter.GetBytes(optionEvent.Y));
        this.is_jumping = optionEvent.OptType == (int)OptType.Jump;

        // logic position 
        this.DoMovementEvent((float)GameZygote.LOGIC_DELTA_TIME * 0.001f);
    }

    private void SyncLastDeadEvent(OptionEvent optionEvent)
    {
        this.DoDieEvent();
    }

    private void JumpToNextMovementEvent(OptionEvent optionEvent)
    {
        SyncLastMovementEvent(optionEvent);
    }

    private void JumpToNextDeadEvent(OptionEvent optionEvent)
    {
        this.DoDieEvent();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Check if the collider we've hit is part of the tilemap
        if (hit.collider.CompareTag("Ground")) // Make sure to assign "TilemapTag" tag to your tilemap collider
        {
            Debug.Log("Collided with Tilemap!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("FinishPoint"))
        {
            if (!is_ghost)
            {
                //Debug.Log("FinishPointTouched!");
                LogicServiceProxy.Instance.PlayerAtFinishPoint();
                collision.gameObject.SetActive(false);
            }
        }
    }
}
