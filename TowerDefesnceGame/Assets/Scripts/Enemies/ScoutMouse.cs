using UnityEngine;

public class ScoutMouse : Enemy
{
    // Uses base Enemy with these Inspector values:
    // Max Health: 40, Speed: 5, Reward: 8, Live Damage: 1
    // Visual: small grey circle, tiny

    public override void OnSpawn()
    {
        base.OnSpawn();
        // Scout mice sometimes dodge — handled in Move override
    }
}