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

    public void UpdateState(float speed, bool isGrounded, bool isDashing, bool isAttacking)
    {
        anim.SetFloat("Speed", speed);
        anim.SetBool("IsDashing", isDashing);
        anim.SetBool("IsAttacking", isAttacking);

        // Jangan langsung ubah IsGrounded kalau baru saja lompat
        if (!isJumping)
            anim.SetBool("IsGrounded", isGrounded);
        else if (isGrounded)
        {
            // Kalau mendarat, reset status lompat
            isJumping = false;
            anim.SetBool("IsGrounded", true);
            anim.ResetTrigger("Jump");
        }

        // Deteksi jatuh
        if (!isGrounded && !isJumping)
        {
            anim.SetBool("IsFalling", true);
        }
        else
        {
            anim.SetBool("IsFalling", false);
        }
    }

    public void TriggerJump()
    {
        if (!isJumping)
        {
            isJumping = true;
            anim.SetBool("IsGrounded", false);
            anim.SetBool("IsFalling", false);
            anim.SetTrigger("Jump");
            Debug.Log("Trigger Jump dipanggil ke Animator");
        }
    }

    public void TriggerDash()
    {
        anim.SetTrigger("Dash");
    }

    public void TriggerAttack()
    {
        anim.SetTrigger("Attack");
    }
}
