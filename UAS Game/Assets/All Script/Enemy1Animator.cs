using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void UpdateState(float speed, bool isAttacking, bool isDead, bool isGrounded)
    {
        anim.SetFloat("Speed", speed);
        anim.SetBool("IsAttacking", isAttacking);
        anim.SetBool("IsDead", isDead);
        anim.SetBool("IsGrounded", isGrounded);
    }
    
    public void TriggerGetHit()
    {
        anim.SetTrigger("GetHit");
    }

    public void TriggerDeath()
    {
        anim.SetTrigger("Death");
    }

    public void TriggerDash()
    {
        anim.SetTrigger("Dash");
    }
}