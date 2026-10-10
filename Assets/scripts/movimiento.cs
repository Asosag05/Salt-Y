using UnityEngine;

public class movimiento : MonoBehaviour
{
    [SerializeField] float speed = 7f;
    [SerializeField] float jumpForce = 12f;
    [SerializeField] LayerMask groundLayer;

    [SerializeField] float jumpCutMultiplier = 0.5f;

    [SerializeField] float coyoteTime = 0.15f;
    private float coyoteCounter;

    [SerializeField] float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;

    private Rigidbody2D rb;
    private BoxCollider2D col;
    private float direction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
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

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            coyoteCounter = 0f;
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(direction*speed, rb.linearVelocity.y);

        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
        }
    }

    bool IsGrounded()
    {
        Vector2 boxSize = new Vector2(col.bounds.size.x * 0.8f, 0.05f);
        Vector2 origin = new Vector2(col.bounds.center.x, col.bounds.min.y);
        return Physics2D.BoxCast(origin, boxSize, 0f, Vector2.down, 0.05f, groundLayer);
    }

}