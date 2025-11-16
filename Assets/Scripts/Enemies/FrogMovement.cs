using System.Collections.Generic;
using UnityEngine;

public enum FrogState
{
    PATROL,
    CHASE,
    RESET,
    STUN
}
public enum JumpState
{
    IDLE = 0,
    JUMPING = 1,
    FALLING = 2
}
public class FrogMovement : MonoBehaviour
{
    [Header("Croak")]
    bool isCroakIdle = true;
    float croakTimer;
    float timeTilNextCroak = 7f;

    [Header("Data")]
    int currentPoint;
    float currentSpeed;
    Transform target;
    FrogState currentState = FrogState.PATROL;

    [Header("Jumping")]
    [SerializeField] float jumpForceX;
    [SerializeField] float jumpForceY;
    [SerializeField] Transform groundCheck;
    bool isJumpIdle = true;
    bool isGrounded;
    float jumpTimer;
    JumpState currentJumpState = JumpState.IDLE;

    [Header("Patrol")]
    [SerializeField] float patrolSpeed = 3f;
    [SerializeField] List<Transform> patrolPoints;
    bool reachedDestination;
    
    [Header("Combat")]
    [SerializeField] float chaseSpeed = 1.5f;
    [SerializeField] float jumpLengthModifier = 1.5f;
    [SerializeField] float jumpHeightModifier = .75f;
    float combatCheckDistance = 3f;

    [Header("Reset")]
    float chaseDuration = 2.5f;
    float chaseTimer;
    Transform resetTarget;
    
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
            HandleInAirStates();
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

                if (IsTimeToJump()) 
                { 
                    Chase();
                }

                if (player == null)
                {
                    if (chaseTimer > 0)
                    {
                        chaseTimer -= Time.fixedDeltaTime;
                    }
                    else
                    {
                        SetReset();
                    }
                }
                else
                {
                    chaseTimer = chaseDuration;
                }

                break;
            case FrogState.RESET:
                
                if (IsTimeToJump())
                {
                    ResetFrog();
                }

                break;
            case FrogState.STUN:
                //Stun();
                break;
        }
        anim.SetInteger("jumpState", (int)currentJumpState);
    }
    
    private bool CheckIsGrounded()
    {
        bool wasGrounded = isGrounded;

        isGrounded = Physics2D.Raycast(groundCheck.position, Vector2.down, 
                                       GameLayers.i.GroundCheckDistance, GameLayers.i.GroundLayer);

        // This is a debugging visual that you will only see in the Scene window. It�s not necessary, but often helpful. It will show the BoxCast as green when grounded and red when not grounded
        //DebugDrawBoxCast(groundCheck.transform.position, groundCheckBox, 0f,
                         //Vector2.down, 0, isGrounded ? Color.green : Color.red);
        return wasGrounded;
    }
    private bool IsTimeToJump()
    {
        if (isJumpIdle & isCroakIdle)
        {
            jumpTimer -= Time.fixedDeltaTime;

            if (jumpTimer <= 0f)
            {
                isJumpIdle = false;
                UpdateFacing();
                return true;
            }
        }

        return false;
    }

    private void HandleLanding(bool wasGrounded)
    {
        if (!wasGrounded)
        {
            isJumpIdle = true;
            jumpTimer = currentSpeed;
            rb.linearVelocity = Vector2.zero;
            currentJumpState = JumpState.IDLE;
        }
    }
    private void HandleInAirStates()
    {
        if (rb.linearVelocity.y > 0.1f)
        {
            currentJumpState = JumpState.JUMPING;
        }
        else if (rb.linearVelocity.y < -0.1f)
        {
            currentJumpState = JumpState.FALLING;
        }
    }

    private void SetPatrol()
    {
        currentState = FrogState.PATROL;
        currentSpeed = patrolSpeed;
        resetTarget = null;
    }
    private void SetChase(Transform player)
    {
        resetTarget = target;

        currentState = FrogState.CHASE;
        target = player;
        currentSpeed = chaseSpeed;
        chaseTimer = chaseDuration;
        jumpTimer = 0;
    }
    private void SetReset()
    {
        currentState = FrogState.RESET;
        target = resetTarget;
    }
    private void Patrol()
    {
        int direction = (target.position.x < transform.position.x) ? -1 : 1;
        rb.linearVelocity = new Vector2(jumpForceX * direction, jumpForceY);
    }
    private void Chase()
    {
        int direction = (target.position.x < transform.position.x) ? -1 : 1;
        rb.linearVelocity = new Vector2(jumpForceX * direction * jumpLengthModifier, 
                                        jumpForceY * jumpHeightModifier);

    }
    private void ResetFrog()
    {
        reachedDestination = Vector2.Distance(target.position, transform.position) < 2f;
        if (reachedDestination)
        {
            SetPatrol();
        }

        int direction = (target.position.x < transform.position.x) ? -1 : 1;
        rb.linearVelocity = new Vector2(jumpForceX * direction, jumpForceY);
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
    private void UpdateFacing()
    {
        Vector3 facing = target.position - transform.position;

        if (facing.x > 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (facing.x < 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }


    private void PlayCroakAnimation()
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
