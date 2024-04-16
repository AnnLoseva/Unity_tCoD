using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    private bool isWallSliding = false; // Check of WallSliding
    private bool isWallJumping = false;
    private float wallJumpingTime = 0.2f;
    private int  wallJumpingDirection = 1;
    private float wallJumpingCounter = 0;
    private float wallJumpingDuration = 2f;
    private enum MovementState { idle, running, jumping, falling } // Animation States



    [SerializeField] private AudioSource jumpSoundEffect;
    [SerializeField] private LayerMask jumpableGround; // Ground Collision
    [SerializeField] private LayerMask climbableWall; // Ground Collision
    [SerializeField] private Transform wallCheck; // Point of wallCheck

    [SerializeField] private float moveSpeed = 7f; // Movement Speed
    [SerializeField] private float jumpForce = 20f; // Jump Force

    [SerializeField] private float airJumpForce = 15f; // Force of jumps in air
    [SerializeField] private int airJumpMaxNum = 1; // Max number of air jumps

    [SerializeField] private float floatGravity = 2f; //Gravity while floating
    [SerializeField] private float regularGravity = 4f; // Regular gravity

    [SerializeField] private float wallSlidingSpeed = 2f; // Speed of Wall Slide
    [SerializeField] private Vector2 wallJumpingPower = new Vector2(20f, 16f);



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
        // Getting X input
        dirX = Input.GetAxisRaw("Horizontal");

        //Moving Horizontaly
        if (isWallSliding==false)
        {
            rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
        }

        UpdateAnimationState();// Calling UpdateAnimationState
        //JumpsAndFloating();
        WallSlide();
        WallJump();
    }

    // Animation Run/Idle
    private void UpdateAnimationState()
    {
        MovementState state;

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

    //private void JumpsAndFloating()
    //{
    //    // Regular Jump from Ground
    //    if (Input.GetButtonDown("Jump") && IsGrounded())
    //    {
    //        jumpCount = 0;
    //        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    //        //jumpSoundEffect.Play();

    //    }
    //    // Air Jump
    //    else if (Input.GetButtonDown("Jump") && !IsGrounded() && jumpCount < airJumpMaxNum)
    //    {
    //        jumpCount++;
    //        rb.velocity = new Vector2(rb.velocity.x, airJumpForce);
    //        //jumpSoundEffect.Play();

    //    }

    //    // Floating
    //    if (Input.GetButton("Jump") && !IsGrounded() && rb.velocity.y < 0)
    //    {

    //        rb.gravityScale = floatGravity;
    //        if (Input.GetButtonDown("Jump"))
    //        {
    //            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.3f);
    //        }
    //    }
    //    else if (!Input.GetButton("Jump") || IsGrounded())
    //    {
    //        rb.gravityScale = regularGravity;
    //    }
    //}

    private void WallSlide()
    {
        if(!IsGrounded() && IsWalled())
        {
            isWallSliding = true;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
           
        }
        else
        {
            print("Not on Wall");
            isWallSliding = false;
            
        }
    }

    private void WallJump()
    {
        if(isWallSliding == true) 
        {
            isWallJumping = false;
            if(sprite.flipX == true)
            { wallJumpingDirection = -1; }
            else { wallJumpingDirection = 1; }

            wallJumpingCounter = wallJumpingTime;

        }
        else
        {
            wallJumpingCounter = Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && isWallSliding == true)
        {
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpingPower.x * wallJumpingDirection, wallJumpingPower.y);
            wallJumpingCounter = 0f;
            CancelInvoke(nameof(StopWallJumping));
        }

        if (sprite.flipX == true && wallJumpingDirection == -1 && isWallJumping)
        {
            sprite.flipX = false;
        }
        else if(sprite.flipX == false && wallJumpingDirection == 1 && isWallJumping)
        {
            sprite.flipX = true;
        }

        Invoke(nameof(StopWallJumping), wallJumpingDuration);
    }
    
    private void StopWallJumping()
    {
       isWallJumping = false;
    }

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, 0.3f, jumpableGround);
    }

    private bool IsWalled()
    {
        bool isWalled;

        if (Physics2D.OverlapCircle(wallCheck.position, 0.2f, jumpableGround))
        {
            isWalled = true;
        }

        if (Physics2D.OverlapCircle(wallCheck.position, 0.2f, climbableWall)) 
        {
            isWalled = true;
        }
        else
        {
            isWalled = false;
        }

        return isWalled;
    }
}



