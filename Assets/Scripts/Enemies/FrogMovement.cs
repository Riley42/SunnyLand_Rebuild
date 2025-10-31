using System.Collections.Generic;
using UnityEngine;

public enum FrogState
{
    PATROL,
    CHASE,
    RESET,
    STUN
}
public class FrogMovement : MonoBehaviour
{
    bool isCroakIdle = true;
    float croakTimer;
    float timeTilNextCroak = 7f;

    bool isJumpIdle = true;
    bool isGrounded;
    int currentPoint;
    float jumpTimer;
    float currentSpeed;
    float combatCheckDistance = 3f;
    Transform target;
    FrogState currentState = FrogState.PATROL;

    [SerializeField] float patrolSpeed = 3f;
    [SerializeField] float chaseSpeed = 1.5f;
    [SerializeField] float jumpForceX;
    [SerializeField] float jumpForceY;
    [SerializeField] List<Transform> patrolPoints;
    [SerializeField] Transform groundCheck;

    Animator anim;
    Rigidbody2D rb;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        croakTimer = Random.Range(timeTilNextCroak - 1.5f, timeTilNextCroak + 1.5f);
        currentSpeed = patrolSpeed;
        jumpTimer = currentSpeed;
        target = patrolPoints[0];
    }

    private void Update()
    {
        if (currentState == FrogState.PATROL)
            PlayCroakAnimation();
    }
    private void FixedUpdate()
    {
        var player = CheckForPlayer();
        bool wasGrounded = CheckIsGrounded();
        if (isGrounded)
        {
            HandleLanding(wasGrounded);
        }
        else
        {
            // HandleInAirState();
        }


        switch (currentState)
            {
                case FrogState.PATROL:

                    if (IsTimeToJump())
                    {
                        Patrol();
                    }

                    if (player != null)
                    {
                        SetChase(player);
                    }
                    break;
                case FrogState.CHASE:

                    //Chase();

                    //if (player == null)
                    //{
                    //    if (chaseTimer > 0)
                    //    {
                    //        chaseTimer -= Time.fixedDeltaTime;
                    //    }
                    //    else
                    //    {
                    //        SetReset();
                    //    }
                    //}
                    //else
                    //{
                    //    chaseTimer = chaseDuration;
                    //}

                    break;
                case FrogState.RESET:

                    //ResetPossum();

                    break;
                case FrogState.STUN:
                    //Stun();
                    break;
            }
    }
    bool CheckIsGrounded()
    {
        bool wasGrounded = isGrounded;

        isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, 
                                       GameLayers.i.GroundCheckDistance, GameLayers.i.GroundLayer);

        // This is a debugging visual that you will only see in the Scene window. It�s not necessary, but often helpful. It will show the BoxCast as green when grounded and red when not grounded
        //DebugDrawBoxCast(groundCheck.transform.position, groundCheckBox, 0f,
                         //Vector2.down, 0, isGrounded ? Color.green : Color.red);
        return wasGrounded;
    }
    bool IsTimeToJump()
    {
        if (isJumpIdle & isCroakIdle)
        {
            jumpTimer -= Time.fixedDeltaTime;

            if (jumpTimer <= 0f)
            {
                isJumpIdle = false;
                return true;
            }
        }
        return false;
    }
    private Transform CheckForPlayer()
    {
        Debug.DrawRay(transform.position, Vector2.left * transform.localScale.x * combatCheckDistance, Color.red);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.left * transform.localScale.x, combatCheckDistance, GameLayers.i.PlayerLayer);
        if (hit)
        {
            return hit.transform;
        }
        return null;
    }

    void HandleLanding(bool wasGrounded)
    {
        if (!wasGrounded)
        {
            isJumpIdle = true;
            jumpTimer = currentSpeed;
            rb.linearVelocity = Vector2.zero;
        }
    }
    
    void SetChase(Transform player)
    {
        currentState = FrogState.CHASE;
        target = player;
        currentSpeed = chaseSpeed;
    }
    void Patrol()
    {
        int direction = (target.position.x < transform.position.x) ? -1 : 1;
        rb.linearVelocity = new Vector2(jumpForceX * direction, jumpForceY);
    }

    void PlayCroakAnimation()
    {
        if (isJumpIdle & isCroakIdle)
        {
            croakTimer -= Time.deltaTime;
        }

        if (croakTimer <= 0f)
        {
            anim.SetTrigger("croak");
            isCroakIdle = false;
            croakTimer = Random.Range(timeTilNextCroak - 1.5f, timeTilNextCroak + 1.5f);
        }
    }
    void ResetCroakIdleAnimationEvent()
    {
        isCroakIdle = true;
    }

}
