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


    [SerializeField] float dashSpeed = 18f;
    [SerializeField] float dashDuration = 0.15f;
    [SerializeField] float dashCooldown = 0.8f;
    [SerializeField] float doubleTapTime = 0.25f;

    public LayerMask groundLayer;

    private float coyoteCounter;
    private float jumpBufferCounter;
    private DistanceJoint2D distanceJoint;
    private Rigidbody2D rb;
    private BoxCollider2D col;
    private float direction;

    private bool isDashing;
    private float dashTimeLeft;
    private float dashCooldownCounter;
    private float dashDirection;
    private float originalGravity;
    private bool canAirDash = true;

    // Detección de doble pulsación en teclado
    private float lastTapTime = -1f;
    private float lastTapDirection = 0f;

    //Variables para los tests
    public float testHorizontalInput = 0f;
    public bool testJumpInputDown = false;
    public bool testJumpInputUp = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        distanceJoint = GetComponent<DistanceJoint2D>();
        originalGravity = rb.gravityScale;
    }

   void Update()
    {
        direction = testHorizontalInput != 0f ? testHorizontalInput : Input.GetAxisRaw("Horizontal");
        // Comprobamos si estamos balanceándonos
        bool isSwinging = distanceJoint != null && distanceJoint.enabled;
        bool grounded = IsGrounded();

        if (dashCooldownCounter > 0f)
        {
            dashCooldownCounter -= Time.deltaTime;
        }

        if (grounded || isSwinging)
        {
            canAirDash = true;
        }

        // Si estamos enganchados, limpiamos el historial de pulsaciones y cancelamos cualquier dash activo
        if (isSwinging)
        {
            lastTapDirection = 0f;
            if (isDashing)
            {
                TerminarDash();
            }
        }

        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0f)
            {
                TerminarDash();
            }
            return;
        }

        DetectarDoblePulsacionTeclado(isSwinging, grounded);

        if (grounded)
        {
            coyoteCounter = coyoteTime;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        // Si estamos balanceándonos, 'W' es solo para enrollar la cuerda, no para saltar
        bool jumpPressed = testJumpInputDown || Input.GetButtonDown("Jump") || (!isSwinging && Input.GetKeyDown(KeyCode.W));

        if (jumpPressed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        bool jumpReleased = testJumpInputUp || Input.GetButtonUp("Jump") || (!isSwinging && Input.GetKeyUp(KeyCode.W));
        if (jumpReleased && rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            coyoteCounter = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
            return;
        }

        bool isSwinging = distanceJoint != null && distanceJoint.enabled;

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


    private void DetectarDoblePulsacionTeclado(bool isSwinging, bool grounded)
    {
        if (isSwinging) return;

        bool pulsoDerecha = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow);
        bool pulsoIzquierda = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow);

        // Si se pulsan ambas direcciones en el mismo frame, ignoramos
        if (pulsoDerecha == pulsoIzquierda) return;

        float direccionPulsada = pulsoDerecha ? 1f : -1f;

        if (dashCooldownCounter <= 0f && (grounded || canAirDash))
        {
            if (direccionPulsada == lastTapDirection && (Time.time - lastTapTime) <= doubleTapTime)
            {
                IniciarDash(direccionPulsada, grounded);
                lastTapDirection = 0f;
            }
            else
            {
                lastTapDirection = direccionPulsada;
                lastTapTime = Time.time;
            }
        }
    }

    private void IniciarDash(float dir, bool grounded)
    {
        isDashing = true;
        dashTimeLeft = dashDuration;
        dashCooldownCounter = dashCooldown;
        dashDirection = dir;

        if (!grounded)
        {
            canAirDash = false;
        }

        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
    }

    private void TerminarDash()
    {
        isDashing = false;
        rb.gravityScale = originalGravity;
        //Para que no conserver la inercia al terminar el dash
        float clampedX = Mathf.Clamp(rb.linearVelocity.x, -maxSpeed, maxSpeed);
        rb.linearVelocity = new Vector2(clampedX, rb.linearVelocity.y);
    }

    public bool IsGrounded()
    {
        Vector2 boxSize = new Vector2(col.bounds.size.x * 0.8f, 0.05f);
        Vector2 origin = new Vector2(col.bounds.center.x, col.bounds.min.y);
        return Physics2D.BoxCast(origin, boxSize, 0f, Vector2.down, 0.05f, groundLayer);
    }
}