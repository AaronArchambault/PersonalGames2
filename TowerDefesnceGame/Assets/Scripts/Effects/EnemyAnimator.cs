using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyAnimator : MonoBehaviour
{
    private Animator anim;

    // Animator parameter hashes — faster than strings
    private static readonly int HurtHash  = Animator.StringToHash("Hurt");
    private static readonly int DieHash   = Animator.StringToHash("Die");
    private static readonly int Phase2Hash = Animator.StringToHash("InPhase2");
    private static readonly int ShieldBreakHash = Animator.StringToHash("ShieldBreak");

    void Awake() => anim = GetComponent<Animator>();

    // Call from Enemy.TakeDamage()
    public void PlayHurt()
    {
        anim.SetTrigger(HurtHash);
    }

    // Call from Enemy.Die()
    public void PlayDeath()
    {
        anim.SetTrigger(DieHash);
    }

    // Call from MegaRoomba / RatKing when entering phase 2
    public void SetPhase2(bool active)
    {
        anim.SetBool(Phase2Hash, active);
    }

    // Call from MegaRoomba when shield breaks
    public void PlayShieldBreak()
    {
        anim.SetTrigger(ShieldBreakHash);
    }
}