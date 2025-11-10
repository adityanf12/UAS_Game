using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator anim;
    private bool isJumping = false;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void UpdateState(float speed, bool isGrounded, bool isDashing, bool isAttacking, float verticalVelocity)
    {
        anim.SetFloat("Speed", speed);
        anim.SetBool("IsGrounded", isGrounded);
        anim.SetBool("IsDashing", isDashing);
        anim.SetBool("IsAttacking", isAttacking);

        // Animasi jatuh biasa
        bool isFalling = !isGrounded && verticalVelocity < -0.1f;
        anim.SetBool("IsFalling", isFalling);

        // Reset trigger lompat ketika mendarat
        if (isGrounded && isJumping)
        {
            isJumping = false;
            anim.ResetTrigger("Jump");
        }
    }

    public void TriggerJump()
    {
        isJumping = true;
        anim.ResetTrigger("Jump");
        anim.SetTrigger("Jump");
        anim.SetBool("IsGrounded", false);
    }

    public void TriggerDash()
    {
        anim.SetTrigger("Dash");
    }

    public void TriggerAttack()
    {
        anim.SetTrigger("Attack");
    }

    public void TriggerFallDeath()
    {
        anim.SetTrigger("FallDeath"); // tambahkan animasi ini di Animator
    }
}
