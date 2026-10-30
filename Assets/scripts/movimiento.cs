using UnityEngine;

public class movimiento : MonoBehaviour
{
    [SerializeField] float maxSpeed = 7f;
    [SerializeField] float groundAcceleration = 10f;
    [SerializeField] float groundDeceleration = 15f;
    [SerializeField] float airControlMultiplier = 0.2f;

    [SerializeField] float jumpForce = 12f;
    [SerializeField] float jumpCutMultiplier = 0.5f;
    [SerializeField] float coyoteTime = 0.15f;
    [SerializeField] float jumpBufferTime = 0.15f;

    [SerializeField] float swingForce = 30f;
    [SerializeField] float maxSwingSpeed = 15f;

    [SerializeField] LayerMask groundLayer;

    private float coyoteCounter;
    private float jumpBufferCounter;
    private SpringJoint2D springJoint;
    private Rigidbody2D rb;
    private BoxCollider2D col;
    private float direction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        springJoint = GetComponent<SpringJoint2D>();
    }

    void Update()
    {
        direction = Input.GetAxisRaw("Horizontal");

        if (IsGrounded())
        {
            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.W))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if ((Input.GetButtonUp("Jump") || Input.GetKeyUp(KeyCode.W)) && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            coyoteCounter = 0f;
        }
    }

    private void FixedUpdate()
    {
        bool isSwinging = springJoint != null && springJoint.enabled;

        if (isSwinging)
        {
            rb.AddForce(Vector2.right * direction * swingForce);
            if (Mathf.Abs(rb.linearVelocity.x) > maxSwingSpeed)
            {
                rb.linearVelocity = new Vector2(Mathf.Sign(rb.linearVelocity.x) * maxSwingSpeed, rb.linearVelocity.y);
            }
        }
        else
        {
            float targetSpeed = direction * maxSpeed;
            float speedDifference = targetSpeed - rb.linearVelocity.x;
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? groundAcceleration : groundDeceleration;

            if (!IsGrounded()) 
            {
                accelRate *= airControlMultiplier;
                
                if (Mathf.Abs(rb.linearVelocity.x) > maxSpeed)
                {
                    if (Mathf.Sign(rb.linearVelocity.x) == Mathf.Sign(targetSpeed) || targetSpeed == 0)
                    {
                        accelRate = 0f; 
                    }
                }
            }

            rb.AddForce(speedDifference * accelRate * Vector2.right, ForceMode2D.Force);
        }

        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
        }
    }

    public bool IsGrounded()
    {
        Vector2 boxSize = new Vector2(col.bounds.size.x * 0.8f, 0.05f);
        Vector2 origin = new Vector2(col.bounds.center.x, col.bounds.min.y);
        return Physics2D.BoxCast(origin, boxSize, 0f, Vector2.down, 0.05f, groundLayer);
    }
}