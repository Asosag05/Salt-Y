using UnityEngine;

public class movimiento : MonoBehaviour
{
    [SerializeField] float speed = 7f;
    [SerializeField] float jumpForce = 12f;
    [SerializeField] LayerMask groundLayer;

    Rigidbody2D rb;
    BoxCollider2D col;
    float moveInput;
    bool jumpPressed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        moveInput = 0f;
        if (Input.GetKey(KeyCode.A)) moveInput -= 1f;
        if (Input.GetKey(KeyCode.D)) moveInput += 1f;

        if (Input.GetKeyDown(KeyCode.Space)) jumpPressed = true;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        if (jumpPressed && IsGrounded())
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        jumpPressed = false;
    }

    bool IsGrounded()
    {
        Vector2 size = new Vector2(col.bounds.size.x * 0.9f, col.bounds.size.y);
        return Physics2D.BoxCast(col.bounds.center, size, 0f, Vector2.down, 0.05f, groundLayer);
    }
}