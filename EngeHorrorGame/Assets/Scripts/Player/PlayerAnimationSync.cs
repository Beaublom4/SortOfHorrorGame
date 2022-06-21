using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationSync : MonoBehaviour
{
    PlayerAttack attackScript;
    private void Awake()
    {
        attackScript = GetComponentInParent<PlayerAttack>();
    }
    public void MeleeHit()
    {
        attackScript.MeleeHitBox();
    }
}
