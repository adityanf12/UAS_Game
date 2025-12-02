using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyAnimator))]
public class EnemyController : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 3;
    private int currentHP;
    public float destroyDelay = 2f; 

    [Header("Movement")]
    public float walkSpeed = 3f;
    public float patrolRange = 5f; 
    private float startX;
    private int walkDirection = 1; 

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
        
        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    void FixedUpdate()
    {
        if (isDead || isAttacking) return;

        bool isGrounded = CheckGroundStatus();
        
        CheckForPlayer();

        if (isChasing)
        {
            HandleChaseAndAttack();
        }
        else
        {
            HandlePatrol();
        }

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

        rb.velocity = new Vector2(walkDirection * walkSpeed, rb.velocity.y);
    }

    private void HandleChaseAndAttack()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer > attackRange)
        {
            isAttacking = false;
            
            float direction = (playerTarget.position.x > transform.position.x) ? 1 : -1;
            walkDirection = (int)direction;
            
            if (spriteRenderer.flipX == (direction > 0)) Flip();
            
            rb.velocity = new Vector2(direction * walkSpeed, rb.velocity.y);
        }
        else
        {
            rb.velocity = Vector2.zero; 
            
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
    }

    private void Flip()
    {
        spriteRenderer.flipX = walkDirection < 0;
    }

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
            rb.velocity = new Vector2(-walkDirection * 3f, rb.velocity.y);
        }
    }

    private void Die()
    {
        isDead = true;
        rb.simulated = false;
        
        enemyAnimator.TriggerDeath();
        
        Destroy(gameObject, destroyDelay);
    }

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

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}