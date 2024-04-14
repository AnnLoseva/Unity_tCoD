using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb; // RigidBody of Player
    private BoxCollider2D coll; // Collistion of Player
    private Animator anim; //Animations
    private float dirX = 0f;  // Movement input
    private SpriteRenderer sprite; // 


    [SerializeField] private AudioSource jumpSoundEffect;
    [SerializeField] private LayerMask jumpableGround; // Ground Collision

    [SerializeField] private float moveSpeed = 7f; // Movement Speed
    [SerializeField] private float jumpForce = 20f; // Jump Force
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

        // Jump
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {


            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpSoundEffect.Play();

        }

        UpdateAnimationState();// Calling UpdateAnimationState

        if(IsGrounded())
        {
            Debug.Log("Grounded");
        }
        else { Debug.Log("Not"); }
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

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, 0.3f, jumpableGround);
    }

}

