using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb; // RigidBody of Player
    private BoxCollider2D coll; // Collistion of Player
    private Animator anim; //Animations
    private float dirX = 0f;  // Movement input
    private float dirY = 0f;  // Movement input
    private SpriteRenderer sprite; // 
    private int jumpCount = 0; // Count of jumps in air


    [SerializeField] private AudioSource jumpSoundEffect;
    [SerializeField] private LayerMask jumpableGround; // Ground Collision
    [SerializeField] private LayerMask climbableWall; // Ground Collision

    [SerializeField] private float moveSpeed = 7f; // Movement Speed
    [SerializeField] private float jumpForce = 20f; // Jump Force
    [SerializeField] private int airJumpMaxNum = 1; // Max number of air jumps
    [SerializeField] private float airJumpForce = 15f; // Force of jumps in air
    [SerializeField] private float floatGravity = 2f; //Gravity while floating
    [SerializeField] private float regularGravity = 4f; // Regular gravity
    private enum MovementState { idle, running, jumping, falling } // Animation States


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
        rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);

        UpdateAnimationState();// Calling UpdateAnimationState
        JumpsAndFloating();
        WallClimb();
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
        }

        //Left
        else if (dirX < 0f)
        {
            state = MovementState.running;
            sprite.flipX = false;

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
        // Air Jump
        else if (Input.GetButtonDown("Jump") && !IsGrounded() && jumpCount < airJumpMaxNum)
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

    private void WallClimb()
    {
        if( Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.left, 0.3f, climbableWall) || Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.right, 0.3f, jumpableGround))
        {
            rb.gravityScale = 0;
            jumpCount = 0;
            dirY = Input.GetAxisRaw("Vertical");

            //Moving Horizontaly
            if (dirY != 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, dirY * moveSpeed);
            }

        }
        else
        {
            rb.gravityScale = regularGravity;
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, 0.3f, jumpableGround);
    }

}

