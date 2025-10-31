using UnityEngine;

public enum FoxJumpState
{
    IDLE = 0,
    JUMP = 1,
    FALL = 2
}
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] bool canMove = true;
    [SerializeField] float runSpeed;

    [Header("Jumping")]
    [SerializeField] bool isJumping;
    [SerializeField] float jumpForce;
    [SerializeField] float maxJumpTime;
    [SerializeField] float jumpTimeCounter;
    [SerializeField] float fallMultiplier = 2.5f;
    [SerializeField] float lowJumpMultiplier = 2.0f;
    [SerializeField] float maxFallSpeed = 20f;
    [SerializeField] Transform groundCheck;
    [SerializeField] FoxJumpState currentJumpState;

    [Header("Advanced Jumping")]
    [SerializeField] bool canDoubleJump;

    bool isGrounded;
    bool isFacingRight = true;
    float horizontal;
    Vector3 velocity;
    Vector2 groundCheckBox;
    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;

    public bool CanMove { get { return canMove; } set { canMove = value; } }

    public FoxJumpState CurrentJumpState { get { return currentJumpState; } }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
        groundCheckBox = new Vector2(sr.bounds.size.x / 2, GameLayers.i.GroundCheckDistance);
    }
    private void Update()
    {
        HandleMovementInputs();
        HandleJumpInputs();
    }
    private void FixedUpdate()
    {
        bool wasGrounded = CheckIsGrounded();

        if (isGrounded)
        {
            if (!wasGrounded)
            {
                HandleLanding();
            }
        }
        else
        {
            HandleInAirStates();
        }

        anim.SetInteger("jumpState", (int)currentJumpState);
        HandleFoxMovement();
    }

    void HandleMovementInputs()
    {
        horizontal = Input.GetAxisRaw("Horizontal") * runSpeed;
        anim.SetFloat("speed", Mathf.Abs(horizontal));
    }
    void HandleJumpInputs()
    {
        if (!isJumping)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (canDoubleJump)
                { 
                    canDoubleJump = isGrounded;
                    isJumping = true;
                    jumpTimeCounter = maxJumpTime;
                }
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.Space) && jumpTimeCounter > 0)
            {
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                jumpTimeCounter = 0;
                isJumping = false;
            }
        }

    }
    void HandleFoxMovement()
    {
        if (!canMove) return;
        if (isGrounded)
        {
            Vector2 targetVelocity = new Vector2(runSpeed * horizontal * Time.fixedDeltaTime, rb.linearVelocity.y);
            rb.linearVelocity = Vector3.SmoothDamp(rb.linearVelocity, targetVelocity, ref velocity, 0f);

            // Initial Jump
            if (isJumping)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                isGrounded = false;
            }
            UpdateFoxFacing();
        }
        else
        {
            // Continue jumping up
            if (isJumping)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }
            // Falling down
            else
            {
                HandleGravity();
            }
        }        
    }
    void HandleLanding()
    {
        isJumping = false;
        currentJumpState = FoxJumpState.IDLE;
        canDoubleJump = true;
    }
    void HandleInAirStates()
    {
        float verticalVelocity = rb.linearVelocity.y;

        if (verticalVelocity > 0.1f)
        {
            currentJumpState = FoxJumpState.JUMP;
        }
        else if (verticalVelocity < -0.1f)
        {
            currentJumpState = FoxJumpState.FALL;
        }
    }
    void HandleGravity()
    {
        Vector2 velocity = rb.linearVelocity;

        // FALLING: Apply extra gravity to fall faster
        if (velocity.y < 0)
        {
            velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        // SHORT-HOP: Apply lighter gravity if the player releases jump early
        else if (velocity.y > 0 && !isJumping)
        {
            velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }

        // Clamp max fall speed (remember: downward velocity is negative)
        velocity.y = Mathf.Clamp(velocity.y, -maxFallSpeed, float.MaxValue);

        rb.linearVelocity = velocity;
    }
    bool CheckIsGrounded()
    {
        bool wasGrounded = isGrounded;

        isGrounded = Physics2D.BoxCast(groundCheck.transform.position, groundCheckBox, 0f,
                                       Vector2.down, 0, GameLayers.i.GroundLayer);

        // This is a debugging visual that you will only see in the Scene window. It�s not necessary, but often helpful. It will show the BoxCast as green when grounded and red when not grounded
        DebugDrawBoxCast(groundCheck.transform.position, groundCheckBox, 0f,
                         Vector2.down, 0, isGrounded ? Color.green : Color.red);
        return wasGrounded;
    }
    void UpdateFoxFacing()
    {
        if (horizontal > 0 && !isFacingRight ||
            horizontal < 0 && isFacingRight)
        {
            isFacingRight = !isFacingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }



    private void DebugDrawBoxCast(Vector3 origin, Vector2 size, float angle, Vector2 direction, float distance, Color color)
    {
        Vector2 halfSize = size / 2;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // Calculate box corners at start
        Vector2 topLeft = origin + rotation * new Vector2(-halfSize.x, halfSize.y);
        Vector2 topRight = origin + rotation * new Vector2(halfSize.x, halfSize.y);
        Vector2 bottomLeft = origin + rotation * new Vector2(-halfSize.x, -halfSize.y);
        Vector2 bottomRight = origin + rotation * new Vector2(halfSize.x, -halfSize.y);

        // Calculate box corners at end
        Vector2 offset = direction.normalized * distance;
        Vector2 topLeftEnd = topLeft + offset;
        Vector2 topRightEnd = topRight + offset;
        Vector2 bottomLeftEnd = bottomLeft + offset;
        Vector2 bottomRightEnd = bottomRight + offset;

        // Draw lines for start box
        Debug.DrawLine(topLeft, topRight, color);
        Debug.DrawLine(topRight, bottomRight, color);
        Debug.DrawLine(bottomRight, bottomLeft, color);
        Debug.DrawLine(bottomLeft, topLeft, color);

        // Draw lines for end box
        Debug.DrawLine(topLeftEnd, topRightEnd, color);
        Debug.DrawLine(topRightEnd, bottomRightEnd, color);
        Debug.DrawLine(bottomRightEnd, bottomLeftEnd, color);
        Debug.DrawLine(bottomLeftEnd, topLeftEnd, color);

        // Draw connecting lines between start and end boxes
        Debug.DrawLine(topLeft, topLeftEnd, color);
        Debug.DrawLine(topRight, topRightEnd, color);
        Debug.DrawLine(bottomLeft, bottomLeftEnd, color);
        Debug.DrawLine(bottomRight, bottomRightEnd, color);
    }
}
