using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerAnimator))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 9f;
    public float jumpForce = 12f;
    public int maxJumpCount = 2;

    [Header("Dash Settings")]
    public float dashForce = 500f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.5f;
    private bool isDashing = false;
    private float dashStartTime;
    private float lastDashTime = -999f;

    [Header("Attack Settings")]
    public GameObject attackHitbox;
    public float attackDuration = 0.3f;
    private bool isAttacking = false;
    private float attackStartTime;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded = false;
    private bool wasGrounded = false;

    [Header("Game Over Settings")]
    public float fallThresholdY = -10f; 
    private bool isFallingToDeath = false;

    [Header("Audio Clips")]
    public AudioClip jumpSound;
    public AudioClip dashSound;
    public AudioClip attackSound;
    public AudioClip gameOverSound; 
    private AudioSource audioSource;

    [Header("Camera Settings")]
    public float minGroundY = 0f; 
    public float cameraMinYOffset = 3f; 
    private float cameraMinY;

    private int jumpCount = 0;
    private float lastGroundedTime = 0f;
    private float coyoteTimeThreshold = 0.15f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private PlayerAnimator playerAnimator;
    private Camera mainCamera;
    private GameManager gameManager;

    private float lastLeftTap = 0f;
    private float lastRightTap = 0f;
    private float doubleTapTime = 0.3f;
    private bool isRunning = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerAnimator = GetComponent<PlayerAnimator>();
        mainCamera = Camera.main;

        audioSource = GetComponent<AudioSource>(); 
        ///if (audioSource == null)
        ///    audioSource = gameObject.AddComponent<AudioSource>();

        gameManager = FindObjectOfType<GameManager>();

        if (attackHitbox != null)
            attackHitbox.SetActive(false);

        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform);
            gc.transform.localPosition = new Vector3(0, -0.5f, 0);
            groundCheck = gc.transform;
        }

        cameraMinY = minGroundY + cameraMinYOffset;
    }

    void Update()
    {
        if (!isFallingToDeath && transform.position.y < fallThresholdY)
        {
            isFallingToDeath = true;
            playerAnimator.TriggerFallDeath(); 
            rb.simulated = false; 
            Invoke(nameof(HandleGameOver), 1.2f);
        }

        HandleTimers();
        HandleCameraFollow();
    }

    void FixedUpdate()
    {
        if (isFallingToDeath) return;

        CheckGroundStatus();
        HandleMovement();
        HandleActions();

        GameInput.ResetAksi();

        playerAnimator.UpdateState(
            Mathf.Abs(rb.velocity.x),
            isGrounded,
            isDashing,
            isAttacking,
            rb.velocity.y,
            isRunning
        );
    }

    private void CheckGroundStatus()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        if (!isGrounded && Mathf.Abs(rb.velocity.y) < 0.05f)
            isGrounded = true;

        if (isGrounded)
        {
            lastGroundedTime = Time.time;
            jumpCount = 0;
        }
    }

    private void HandleMovement()
    {
        if (isDashing) return;

        float horizontalInput = 0f;
        if (GameInput.MoveLeft)
        {
            horizontalInput = -1f;
            CheckRunTap(ref lastLeftTap);
        }
        else if (GameInput.MoveRight)
        {
            horizontalInput = 1f;
            CheckRunTap(ref lastRightTap);
        }
        else
        {
            isRunning = false;
        }

        float currentSpeed = isRunning ? runSpeed : walkSpeed;
        rb.velocity = new Vector2(horizontalInput * currentSpeed, rb.velocity.y);

        if (horizontalInput != 0 && spriteRenderer != null)
            spriteRenderer.flipX = horizontalInput < 0;
    }

    private void CheckRunTap(ref float lastTapTime)
    {
        if (Time.time - lastTapTime < doubleTapTime)
            isRunning = true;
        lastTapTime = Time.time;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void HandleActions()
    {
        bool canJump = (isGrounded || Time.time - lastGroundedTime <= coyoteTimeThreshold || jumpCount < maxJumpCount);

        if (GameInput.JumpPressed && canJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpCount++;
            isGrounded = false;
            playerAnimator.TriggerJump();
            PlaySound(jumpSound);
        }

        // Dash
        if (GameInput.DashPressed && Time.time - lastDashTime >= dashCooldown && !isDashing)
        {
            PerformDash();
        }

        // Attack
        if (GameInput.AttackPressed && !isAttacking)
        {
            DoAttackAction();
        }
    }

    private void PerformDash()
    {
        isDashing = true;
        dashStartTime = Time.time;
        lastDashTime = Time.time;

        float dashDirection = spriteRenderer.flipX ? -1f : 1f;
        rb.velocity = new Vector2(dashDirection * dashForce * Time.fixedDeltaTime, 0);
        playerAnimator.TriggerDash();
        PlaySound(dashSound);
    }

    private void DoAttackAction()
    {
        isAttacking = true;
        attackStartTime = Time.time;
        playerAnimator.TriggerAttack();
        PlaySound(attackSound);

        if (attackHitbox != null)
            attackHitbox.SetActive(true);
    }

    private void HandleTimers()
    {
        if (isDashing && Time.time - dashStartTime >= dashDuration)
        {
            isDashing = false;
        }

        if (isAttacking && Time.time - attackStartTime >= attackDuration)
        {
            isAttacking = false;
            if (attackHitbox != null)
                attackHitbox.SetActive(false);
        }
    }

    private void HandleCameraFollow()
    {
        if (mainCamera == null) return;

        if (isFallingToDeath) return;

        float targetY = transform.position.y;

        Vector3 targetPos = new Vector3(
            transform.position.x,
            targetY,
            mainCamera.transform.position.z
        );

        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            targetPos,
            Time.deltaTime * 5f
        );
    }

    private void HandleGameOver()
    {
        Debug.Log("Game Over - Player jatuh!");

        PlaySound(gameOverSound);
        
        if (gameManager != null)
        {
            gameManager.GameOver();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }

        // Visualisasi batas jatuh
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-100, fallThresholdY, 0), new Vector3(100, fallThresholdY, 0));
    }
}
