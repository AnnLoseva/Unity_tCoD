using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Tilemaps;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Variables

    private Rigidbody2D rb; // RigidBody of Player
    private BoxCollider2D coll; // Collistion of Player
    private Animator anim; //Animations
    private float dirX = 0f;  // Movement input
    private float dirY = 0f;  // Movement input
    private SpriteRenderer sprite; // 
    private int jumpCount = 0; // Count of jumps in air
    private float runJumpTimer; // Timer of running for high jump
    private float varJumpForce; // Changable Jump Force
    bool right = false;
    public MovementForCamera cameraMovement;
    private enum MovementState { idle, running, jumping, falling } // Animation States


    [Header("Walls")]
    [SerializeField] private float wallSlidingSpeed;
    [SerializeField] private float wallJumpDuration;
    [SerializeField] private Vector2 wallJumpForce;
    [SerializeField] private Transform wallCheck; // Point of wallCheck
    private bool isWalljumping;
    private bool isSliding;
    private float timerWallJump;


    [Header("Audio")]
    [SerializeField] private AudioSource jumpSoundEffect;


    [Header("LayerMasks")]
    [SerializeField] private LayerMask jumpableGround; // Ground Collision
    [SerializeField] private LayerMask climbableWall; // Ground Collision


    [Header("Move")]
    [SerializeField] private float moveSpeed = 7f; // Movement Speed
    [SerializeField] private float jumpForce = 20f; // Jump Force
    [SerializeField] private float runJumpTime = 1f; // Running time needed to jump higher
    [SerializeField] private float highJumpModificator = 2f; // Modificator of high jump
    [SerializeField] private float climbSpeed = 4f; // Speed of climbing


    [Header("Airjumps")]
    [SerializeField] private float airJumpForce = 15f; // Force of jumps in air
    [SerializeField] private int airJumpMaxNum = 1; // Max number of air jumps


    [Header("Gravity")]
    [SerializeField] private float floatGravity = 2f; //Gravity while floating
    [SerializeField] private float regularGravity = 4f; // Regular gravity

    #endregion Variables

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
        // Move only if no dialogue is plaing
        if (DialogueManager.GetInstance().dialogueIsPlaying)
        { 
            return; 
        }

    

        UpdateAnimationState();
        JumpsAndFloating();
        WallJump();
        Moving();
        ClimbWall();
    }

    // Animation Run/Idle
    private void UpdateAnimationState()
    {
        MovementState state;

        
        // Right
        if (dirX > 0f)
        {
            state = MovementState.running;

            if (!isWalljumping)
            {
                transform.localScale = new Vector3(-1, 1, 1);


                if (IsGrounded() && (runJumpTimer += Time.deltaTime) >= runJumpTime)
                {
                    varJumpForce = jumpForce * highJumpModificator;
                }
                else
                {
                    varJumpForce = jumpForce;
                }

               
            }
        }

        //Left
        else if (dirX < 0f)
        {
            state = MovementState.running;
            if (!isWalljumping)
            {
                transform.localScale = new Vector3(1, 1, 1);
                if (IsGrounded() && (runJumpTimer += Time.deltaTime) >= runJumpTime)
                {
                    varJumpForce = jumpForce * highJumpModificator;
                }
                else
                {
                    varJumpForce = jumpForce;
                }
                if (right == true)
                {
                    right = false;
                }
            }
        }
        //Idle
        else
        {
            state = MovementState.idle;
            runJumpTimer = 0f;
            varJumpForce = jumpForce;
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
            rb.velocity = new Vector2(rb.velocity.x, varJumpForce);
            //jumpSoundEffect.Play();

        }
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

        if(!IsGrounded())
        {
            runJumpTimer = 0;
            varJumpForce = jumpForce;
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
            if (Input.GetButtonDown("Jump")) 
            {
                isWalljumping = true;
                dirX = 0;
                rb.velocity = new Vector2(wallJumpForce.x * transform.localScale.x, wallJumpForce.y);
                jumpCount = 0;
                if (transform.localScale.x == 1)
                {
                    transform.localScale = new Vector3(-1, 1, 1);
                }
                else
                { 
                    transform.localScale = new Vector3(1, 1, 1); 
                }

            }
        }
        if(isWalljumping)
        {

            if((timerWallJump += Time.deltaTime) >= wallJumpDuration)
            {
                if(IsWalled() || IsGrounded() || dirX > 0.1f || dirX < -0.1f)
                {
                    isWalljumping = false;
                    timerWallJump = 0;
                }
            }
           
        }

    }

    private void Moving()
    {

        // Getting X input
        dirX = Input.GetAxisRaw("Horizontal");
       
        if (!isWalljumping)
        {
            //Moving Horizontaly
            rb.velocity = new Vector2(dirX * moveSpeed, rb.velocity.y);
        }
    }

    private void ClimbWall()
    {
        dirY = Input.GetAxis("Vertical");

        if(IsWalled() && !IsGrounded() && dirY != 0f) 
        {
            rb.velocity = new Vector2(rb.velocity.x, dirY * climbSpeed);
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
  
}

    


