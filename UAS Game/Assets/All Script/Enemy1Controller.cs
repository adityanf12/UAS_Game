using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyAnimator))]
public class EnemyController : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 3;
    private int currentHP;
    public float destroyDelay = 2f; // Waktu sebelum objek musuh dihapus setelah mati

    [Header("Movement")]
    public float walkSpeed = 3f;
    public float patrolRange = 5f; // Jarak patroli dari posisi awal
    private float startX;
    private int walkDirection = 1; // 1 = kanan, -1 = kiri

    [Header("Attack & Detection")]
    public float detectionRange = 5f;
    public float attackRange = 1f;
    public float attackCooldown = 1.5f;
    public LayerMask playerLayer;
    
    private float lastAttackTime = -999f;
    private Transform playerTarget;
    private bool isAttacking = false;
    private bool isChasing = false;

    [Header("References")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    // Status
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private EnemyAnimator enemyAnimator;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyAnimator = GetComponent<EnemyAnimator>();
        
        currentHP = maxHP;
        startX = transform.position.x;
        
        // Cari pemain saat game dimulai (opsional, bisa diganti dengan deteksi visual)
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void FixedUpdate()
    {
        if (isDead || isAttacking) return;

        bool isGrounded = CheckGroundStatus();
        
        // 1. Cek Pemain
        CheckForPlayer();

        if (isChasing)
        {
            HandleChaseAndAttack();
        }
        else
        {
            HandlePatrol();
        }

        // 2. Update Animator
        enemyAnimator.UpdateState(
        Mathf.Abs(rb.velocity.x), 
        isAttacking,               
        isDead,
        isGrounded
    );
    }

    private bool CheckGroundStatus()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
    
    private void CheckForPlayer()
    {
        if (playerTarget == null)
        {
            isChasing = false;
            return;
        }
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer <= detectionRange)
        {
            isChasing = true;
        }
        else
        {
            isChasing = false;
        }
    }

    private void HandlePatrol()
    {
        // Pindah arah jika mencapai batas patroli
        if (transform.position.x > startX + patrolRange && walkDirection > 0)
        {
            walkDirection = -1;
            Flip();
        }
        else if (transform.position.x < startX - patrolRange && walkDirection < 0)
        {
            walkDirection = 1;
            Flip();
        }

        // Terapkan kecepatan patroli
        rb.velocity = new Vector2(walkDirection * walkSpeed, rb.velocity.y);
    }

    private void HandleChaseAndAttack()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer > attackRange)
        {
            // Pengejaran
            isAttacking = false;
            
            // Tentukan arah ke pemain
            float direction = (playerTarget.position.x > transform.position.x) ? 1 : -1;
            walkDirection = (int)direction;
            
            // Balik sprite jika perlu
            if (spriteRenderer.flipX == (direction > 0)) Flip();
            
            // Lanjutkan mengejar
            rb.velocity = new Vector2(direction * walkSpeed, rb.velocity.y);
        }
        else
        {
            // Jarak Serang
            rb.velocity = Vector2.zero; // Berhenti bergerak saat menyerang
            
            if (Time.time > lastAttackTime + attackCooldown)
            {
                PerformAttack();
            }
        }
    }

    private void PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // Panggil trigger animasi attack (asumsikan Attack ada di EnemyAnimator)
        // enemyAnimator.TriggerAttack(); 
        
        // Anda perlu implementasi logika untuk mematikan isAttacking setelah animasi selesai
        // dan menjalankan logika damage ke pemain di sini.
    }

    private void Flip()
    {
        spriteRenderer.flipX = walkDirection < 0;
    }

    // Dipanggil oleh objek pemain (atau hitbox)
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHP -= damage;

        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            enemyAnimator.TriggerGetHit();
            // Optional: Tambahkan sedikit dorongan ke belakang saat kena hit
            rb.velocity = new Vector2(-walkDirection * 3f, rb.velocity.y);
        }
    }

    private void Die()
    {
        isDead = true;
        rb.simulated = false; // Hentikan fisika
        // Hentikan semua Coroutine jika ada
        
        enemyAnimator.TriggerDeath();
        
        // Hancurkan objek setelah animasi kematian selesai
        Destroy(gameObject, destroyDelay);
    }

    // Visualisasi jangkauan di Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Visualisasi batas patroli
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(new Vector3(startX + patrolRange, transform.position.y - 0.5f, 0), new Vector3(startX + patrolRange, transform.position.y + 0.5f, 0));
        Gizmos.DrawLine(new Vector3(startX - patrolRange, transform.position.y - 0.5f, 0), new Vector3(startX - patrolRange, transform.position.y + 0.5f, 0));
    }

    // Fungsi tambahan untuk mendeteksi Ground (perlu groundCheck diatur di Editor)
    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}