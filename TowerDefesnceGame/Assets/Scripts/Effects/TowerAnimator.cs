using UnityEngine;

[RequireComponent(typeof(Animator))]
public class TowerAnimator : MonoBehaviour
{
    private Animator anim;

    private static readonly int AttackHash    = Animator.StringToHash("Attack");
    private static readonly int SleepingHash  = Animator.StringToHash("IsSleeping");
    private static readonly int WaveClearHash = Animator.StringToHash("WaveClear");
    private static readonly int PounceHash    = Animator.StringToHash("Pounce");
    private static readonly int PanicHash     = Animator.StringToHash("Panic");
    private static readonly int CastHash      = Animator.StringToHash("Cast");
    private static readonly int InvisHash     = Animator.StringToHash("IsInvisible");

    void Awake() => anim = GetComponent<Animator>();

    void OnEnable()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveComplete += OnWaveClear;
    }

    void OnDisable()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveComplete -= OnWaveClear;
    }

    public void PlayAttack()  => anim.SetTrigger(AttackHash);
    public void PlayPounce()  => anim.SetTrigger(PounceHash);
    public void PlayPanic()   => anim.SetTrigger(PanicHash);
    public void PlayCast()    => anim.SetTrigger(CastHash);

    public void SetSleeping(bool sleeping) => anim.SetBool(SleepingHash, sleeping);
    public void SetInvisible(bool invis)   => anim.SetBool(InvisHash, invis);

    void OnWaveClear() => anim.SetTrigger(WaveClearHash);
}