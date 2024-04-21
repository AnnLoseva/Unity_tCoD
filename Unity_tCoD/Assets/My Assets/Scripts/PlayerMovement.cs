using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Tilemaps;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb; // RigidBody of Player
    private BoxCollider2D coll; // Collistion of Player
    private Animator anim; //Animations
    private float dirX = 0f;  // Movement input
    //private float dirY = 0f;  // Movement input
    private SpriteRenderer sprite; // 
    private int jumpCount = 0; // Count of jumps in air
    private enum MovementState { idle, running, jumping, falling } // Animation States
    public int direction;


    [Header("Walls")]
    [SerializeField] private float wallSlidingSpeed;
    [SerializeField] private float wallJumpDuration;
    [SerializeField] private Vector2 wallJumpForce;
    [SerializeField] private Transform wallCheck; // Point of wallCheck
    private bool isWalljumping;
    private bool canFlip;
    private bool isWallTouch;
    private bool isSliding;


    [Header("Audio")]
    [SerializeField] private AudioSource jumpSoundEffect;


    [Header("LayerMasks")]
    [SerializeField] private LayerMask jumpableGround; // Ground Collision
    [SerializeField] private LayerMask climbableWall; // Ground Collision


    [Header("Move")]
    [SerializeField] private float moveSpeed = 7f; // Movement Speed
    [SerializeField] private float jumpForce = 20f; // Jump Force


    [Header("Airjumps")]
    [SerializeField] private float airJumpForce = 15f; // Force of jumps in air
    [SerializeField] private int airJumpMaxNum = 1; // Max number of air jumps


    [Header("Gravity")]
    [SerializeField] private float floatGravity = 2f; //Gravity while floating
    [SerializeField] private float regularGravity = 4f; // Regular gravity



    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>(); ;
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (DialogueManager.GetInstance().dialogueIsPlaying)
        { 
            return; 
        }

    

        UpdateAnimationState();// Calling UpdateAnimationState
        JumpsAndFloating();
        WallJump();
        Moving();
    }

    // Animation Run/Idle
    private void UpdateAnimationState()
    {
        MovementState state;
       
        
        if (sprite.flipX) { direction = 1; 
        }
        else { direction = -1;}

        // Right
        if (dirX > 0f)
        {
            state = MovementState.running;
            sprite.flipX = true;
            wallCheck.localPosition = new Vector2(0.7f, 1);
        }

        //Left
        else if (dirX < 0f)
        {
            state = MovementState.running;
            sprite.flipX = false;
            wallCheck.localPosition = new Vector2(-0.7f, 1);

        }

        //Idle
        else
        {
            state = MovementState.idle;
        }

        //jump
        if (rb.velocity.y > 0.1f)
        {
            state = MovementState.jumping;
        }

        //fall
        else if (rb.velocity.y < -0.1f)
        {
            state = MovementState.falling;
        }


        // Changing Animations
        anim.SetInteger("state", (int)state);
    }

    private void JumpsAndFloating()
    {
        // Regular Jump from Ground
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            jumpCount = 0;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            //jumpSoundEffect.Play();

        }
        else if (isSliding && Input.GetButtonDown("Jump"))
        {
            isWalljumping = true;
            Invoke("StopWallJump", wallJumpDuration);
        }
        // Air Jump
        else if (Input.GetButtonDown("Jump") && !IsGrounded() && jumpCount < airJumpMaxNum && !isWalljumping && !isSliding)
        {
            jumpCount++;
            rb.velocity = new Vector2(rb.velocity.x, airJumpForce);
            //jumpSoundEffect.Play();

        }

        // Floating
        if (Input.GetButton("Jump") && !IsGrounded() && rb.velocity.y < 0)
        {

            rb.gravityScale = floatGravity;
            if (Input.GetButtonDown("Jump"))
            {
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.3f);
            }
        }
        else if (!Input.GetButton("Jump") || IsGrounded())
        {
            rb.gravityScale = regularGravity;
        }
    }

    private void WallJump()
    {
        if (IsWalled() && !IsGrounded())
        {
            isSliding = true;
        }
        else
        {
            isSliding = false;
        }

        if(isSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
        }
        if(isWalljumping)
        {
            
            if(canFlip)
            {
                sprite.flipX = !sprite.flipX;
                canFlip = false;
                jumpCount = 0;
            }
            rb.velocity = new Vector2(wallJumpForce.x *direction , wallJumpForce.y);
        }
        //else 
        //{
        //    rb.velocity = new Vector2 (direction * moveSpeed, rb.velocity.y);
        //}
    }

    private void StopWallJump()
    {
        isWalljumping = false;
        canFlip = true;
    }

    private void Moving()
    {
        if (!isWalljumping)
        {
            // Getting X input
            dirX = Input.GetAxis("Horizontal");

            //Moving Horizontaly
            rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
        }
    }



    private bool IsGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, 0.3f, jumpableGround);
    }
    private bool IsWalled()
    {
       return Physics2D.OverlapBox(wallCheck.position, new Vector2(0.3f, 1.6f), 0, climbableWall);

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(wallCheck.position, new Vector3(wallCheck.position.x + 3f, wallCheck.position.y, wallCheck.position.z));
    }
}

    


